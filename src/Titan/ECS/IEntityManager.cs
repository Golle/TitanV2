namespace Titan.ECS;

public interface IEntityManager : IService
{
    Entity Create();
    void Destroy(Entity entity);
}
