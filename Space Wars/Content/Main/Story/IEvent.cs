namespace Space_Wars.Content.Main.Story;
public interface IEvent
{
    //True  => Event is still active
    //False => Event is completed
    public bool Update(float _time);
}
