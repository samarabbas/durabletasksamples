namespace DurableTaskSamples.UtilitySignup
{
    public class CustomerAddress
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public int Zip { get; set; }

        public override string ToString()
        {
            return string.Format("{0}, {1}, {2} - {3}", Street, City, State, Zip);
        }
    }
}
