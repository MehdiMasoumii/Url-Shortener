using IdGen;

namespace WebApi.Services;

public class IdGeneratorService(IdGenerator generator)
{
    public long GenerateId()
    {
        return generator.CreateId();
    }
}