using System.ComponentModel.DataAnnotations;

namespace Web.Features.EmployeeManagement.Models;

public class Employee
{
    public int No { get; set; }

    [Required(ErrorMessage = "NIK wajib diisi.")]
    public string Nik { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nama karyawan wajib diisi.")]
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
