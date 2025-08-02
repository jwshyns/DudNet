namespace DudNet.Sample;

public sealed partial class PersonProxy
{
    public PersonProxy(IPerson person)
    {
        _service = person;
    }
}