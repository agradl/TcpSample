namespace TcpSample.Messages
{
    public enum TcpServerMessage
    {
        None = 0,
        CountDown3 = 1,
        CountDown2 = 2,
        CountDown1 = 3,
        Click = 4,
        Lost = 5,
        Won = 6,
        InQueue = 7,
        InGame = 8
    }
}