using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Common.Exceptions.Assets;
using Application.Common.Interfaces;
using Neo4jClient;
using OPD.Logging.Interfaces;
using OPD.Logging.Models;
using OPD.Models.Entities.Assets;
using OPD.Models.Entities.Assets.Base;

namespace Infrastructure.Persistence.Repositories.Neo4j;

public class Neo4jAssetRepository : IAssetRepository
{
    private readonly ILogContext _logger;
    private readonly IBoltGraphClient _client;

    public Neo4jAssetRepository(ILogContext logger, IBoltGraphClient client)
    {
        _logger = logger;
        _client = client;
    }

    
    public async Task<(string FactoryAssetId, string PlantAssetId, bool HasFactoryPlantCombination)>
        ResolveFactoryPlantAsync(string factory, string plant)
    {
        try
        {
            var cypher = _client.Cypher
                .OptionalMatch("(factory:Factory)")
                .Where("factory.AssetId = $factory OR factory.AssetTerm = $factory")
                .WithParam("factory", factory)
                .OptionalMatch("(plant:Plant)")
                .Where("plant.AssetId = $plant OR plant.AssetTerm = $plant")
                .WithParam("plant", plant)
                .With("factory, plant")
                .OptionalMatch("(plant)-[:BELONGS_TO]->(factory)")
                .With(@"factory, plant, COUNT(*) > 0 AND factory IS NOT NULL AND plant IS NOT NULL AS hasFactoryPlantCombination")
                .Return((factory, plant, hasFactoryPlantCombination) => new
                {
                    FactoryAssetId = factory.As<Asset>().AssetId,
                    PlantAssetId = plant.As<Asset>().AssetId,
                    HasFactoryPlantCombination = hasFactoryPlantCombination.As<bool>()
                });

            var result = (await cypher.ResultsAsync).FirstOrDefault();

            return result != null
                ? (result.FactoryAssetId, result.PlantAssetId, result.HasFactoryPlantCombination)
                : (null, null, false);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to resolve provided factory plant combination!", ex);
        }
    }

    
    public async Task<Asset> GetById(string assetId)
    {
        try
        {
            var cypher = _client.Cypher
                .Match("(asset:Asset { AssetId : $assetId })")
                .OptionalMatch("(asset)-[:PHYSICALLY_LOCATED]->(location:Asset)")
                .Where("location:Factory OR location:Plant OR location:Subplant")
                .WithParams(new { assetId })
                .With(@"asset, location, [label IN labels(asset) WHERE NOT label IN ['Asset', 'SubmodelData']] AS filteredLabels")
                .With(@"apoc.map.merge(properties(asset), { Location: location.AssetId, AssetLabels: filteredLabels }) AS result")
                .Return(result => result.As<Asset>());


            //_logger.Info(new Log(), Neo4jLog.ToJson(cypher));

            var result = await cypher.ResultsAsync;

            return result.FirstOrDefault();
        }
        catch (Exception ex)
        {
            throw new AssetGetItemFailedException(_logger, new Log(), ex.Message);
        }
    }
}