using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransportStatic.Models;

[Table("notes")]
public class Note
{
    [Key]
    [Column("note_id")]
    public string Id { get; set; } = null!;

    [Column("note_text")]
    public string Text { get; set; } = null!;

    public static Note ParseColumns(string[] cols)
    {
        return new Note
        {
            Id = cols[0],
            Text = cols[1]
        };
    }
}
