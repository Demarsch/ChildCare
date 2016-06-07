namespace Core.Misc
{
    public interface IChangeTrackerMediator
    {
        IChangeTracker CompositeChangeTracker { get; }
    }
}
