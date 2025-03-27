using Domain;
using GameBrain;

namespace DAL;

public interface IConfigRepository
{
    List<string> GetConfigurationNames();
    Configuration GetConfigurationByName(string name);
    void SaveConfiguration(GameConfiguration config);
    void DeleteConfiguration(string name);
    
    List<Configuration> GetAllConfigurations();
    Configuration? GetConfigurationById(int id);
    bool ConfigurationExists(int? configId);
    void SaveConfiguration(Configuration config);
}