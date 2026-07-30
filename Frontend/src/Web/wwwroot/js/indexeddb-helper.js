window.indexedDbHelper = {
    _db: null,

    openDatabase: function (dbName, version) {
        return new Promise((resolve, reject) => {
            const request = indexedDB.open(dbName, version);
            request.onupgradeneeded = (event) => {
                const db = event.target.result;
                const stores = ['sparta_grading', 'sparta_masterdata', 'legal_contracts', 'portal_applications', 'sync_queue', 'sync_metadata'];
                stores.forEach(storeName => {
                    if (!db.objectStoreNames.contains(storeName))
                        db.createObjectStore(storeName, { keyPath: 'id' });
                });
            };
            request.onsuccess = (event) => { this._db = event.target.result; resolve(); };
            request.onerror = (event) => reject(event.target.error);
        });
    },

    upsert: function (dbName, storeName, json) {
        const item = JSON.parse(json);
        return this._transaction(storeName, 'readwrite', (store) => store.put(item));
    },

    upsertBatch: function (dbName, storeName, jsonArray) {
        const items = JSON.parse(jsonArray);
        return this._transaction(storeName, 'readwrite', (store) => items.forEach(item => store.put(item)));
    },

    getAll: function (dbName, storeName) {
        return this._transaction(storeName, 'readonly', (store) => {
            const request = store.getAll();
            return new Promise((resolve) => { request.onsuccess = () => resolve(JSON.stringify(request.result)); });
        });
    },

    getById: function (dbName, storeName, id) {
        return this._transaction(storeName, 'readonly', (store) => {
            const request = store.get(id);
            return new Promise((resolve) => { request.onsuccess = () => resolve(request.result ? JSON.stringify(request.result) : null); });
        });
    },

    delete: function (dbName, storeName, id) {
        return this._transaction(storeName, 'readwrite', (store) => store.delete(id));
    },

    clearStore: function (dbName, storeName) {
        return this._transaction(storeName, 'readwrite', (store) => store.clear());
    },

    count: function (dbName, storeName) {
        return this._transaction(storeName, 'readonly', (store) => {
            const request = store.count();
            return new Promise((resolve) => { request.onsuccess = () => resolve(request.result); });
        });
    },

    _transaction: function (storeName, mode, callback) {
        return new Promise((resolve, reject) => {
            if (!this._db) { reject(new Error('Database not opened')); return; }
            const tx = this._db.transaction(storeName, mode);
            const store = tx.objectStore(storeName);
            const result = callback(store);
            if (result instanceof Promise) result.then(resolve).catch(reject); else resolve(result);
            tx.oncomplete = () => resolve();
            tx.onerror = (event) => reject(event.target.error);
        });
    }
};
