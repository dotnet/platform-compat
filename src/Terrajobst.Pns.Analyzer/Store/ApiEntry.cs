namespace Terrajobst.Pns.Analyzer.Store
{
    internal struct ApiEntry<T>
    {
        public ApiEntry(string docId, T data)
        {
            DocId = docId;
            Data = data;
        }

        public string DocId { get; }
        public T Data { get; }
    }
}
