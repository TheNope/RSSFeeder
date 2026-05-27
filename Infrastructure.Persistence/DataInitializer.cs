using System.Threading.Tasks;
using Application.Common.Interfaces;
using OPD.Logging.Interfaces;

namespace Infrastructure.Persistence;

public class DataInitializer
{
    private readonly ILogContext _logger;
    private readonly IDatabaseContext _context;
    // private readonly IBoltGraphClient _neo4JClient;

    public DataInitializer(ILogContext logger, IDatabaseContext context /*, IBoltGraphClient neo4jClient*/)
    {
        _logger = logger;
        _context = context;
        // _neo4JClient = neo4jClient;
    }

    public async Task SeedAsync()
    {
        
    }

    
}