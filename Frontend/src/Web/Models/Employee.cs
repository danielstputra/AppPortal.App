namespace Web.Models;

public class Employee
{
    public int No { get; set; }
    public string Nik { get; set; } = string.Empty;
    public string NamaKaryawan { get; set; } = string.Empty;
    public string Jabatan { get; set; } = string.Empty;
    public string Organisasi { get; set; } = string.Empty;
    public string UnitKerja { get; set; } = string.Empty;
    public string SumberData { get; set; } = string.Empty;
    public StatusKaryawan Status { get; set; }
}

public enum StatusKaryawan
{
    Active,
    Resigned,
    Pending
}
