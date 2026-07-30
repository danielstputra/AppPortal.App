window.networkStatusHelper = {
    _dotNetRef: null,
    _methodName: null,
    _onlineHandler: null,
    _offlineHandler: null,

    listen: function (dotNetRef, methodName) {
        this._dotNetRef = dotNetRef;
        this._methodName = methodName;
        this._onlineHandler = () => dotNetRef.invokeMethodAsync(methodName, true);
        this._offlineHandler = () => dotNetRef.invokeMethodAsync(methodName, false);
        window.addEventListener('online', this._onlineHandler);
        window.addEventListener('offline', this._offlineHandler);
    },

    dispose: function () {
        if (this._onlineHandler) {
            window.removeEventListener('online', this._onlineHandler);
            window.removeEventListener('offline', this._offlineHandler);
        }
        this._dotNetRef = null;
    }
};
