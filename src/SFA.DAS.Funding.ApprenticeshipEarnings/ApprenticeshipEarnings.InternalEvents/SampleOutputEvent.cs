namespace SFA.DAS.Funding.Events
{
    public class SampleOutputEvent
    {
        public string Data { get; }

        public SampleOutputEvent(string data)
        {
            Data = data;
        }
    }
}