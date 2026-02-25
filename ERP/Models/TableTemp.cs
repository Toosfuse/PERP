using System.ComponentModel.DataAnnotations.Schema;

public class TableTemp
{
    [Column("id")]
    public int Id { get; set; }

    [Column("persianDate")]
    public string? PersianDate { get; set; }

    [Column("ENglishDat")]
    public DateTime? EnglishDat { get; set; }
}