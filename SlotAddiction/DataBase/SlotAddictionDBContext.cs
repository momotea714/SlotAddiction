using SlotAddiction.Models;
using System.Data.Entity;

namespace SlotAddiction.DataBase
{
    //モデルを変更したらNugetPakegeManagerで以下の二つを実行
    //Add-Migration XXXXXXXXXXXX←善きに計らって
    //Update-Database
    public class SlotAddictionDBContext : DbContext
    {
        public SlotAddictionDBContext()
            : base("SlotAddiction")
        {
        }
        public DbSet<Tempo> Tempos { get; set; }
    }
}
