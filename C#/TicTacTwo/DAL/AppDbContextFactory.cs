using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DAL;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        var connectionString = "Data Source=<%location%>app.db";
        connectionString = connectionString.Replace("<%location%>", FileHelper.BasePath);
        optionsBuilder.UseSqlite(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }

}