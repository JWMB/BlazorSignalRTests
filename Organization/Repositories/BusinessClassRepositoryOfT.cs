namespace Organization.Repositories
{
    public class BusinessClassRepositoryOfT<TBusinessClass, TSerializable> : IRepository<TBusinessClass>
        where TBusinessClass : IDocumentId, IWithSerializable<TSerializable>
        where TSerializable : IDocumentId, IToBusinessObject<TBusinessClass>
    {
        protected readonly RepositoryOfT<TSerializable> serializedRepository;
        protected readonly Dictionary<Guid, TBusinessClass> values = new();

        public BusinessClassRepositoryOfT(RepositoryOfT<TSerializable> serializedRepository)
        {
            this.serializedRepository = serializedRepository;
        }

        private TSerializable ToSerializable(TBusinessClass item) => item.ToSerializable();

        public void Add(TBusinessClass item)
        {
            values.Add(item.Id, item);
            serializedRepository.Add(ToSerializable(item));
        }
        public void Update(TBusinessClass item)
        {
            values[item.Id] = item;
            serializedRepository.Update(ToSerializable(item));
        }

        public TBusinessClass Get(Guid id) => values[id];

        public IEnumerable<TSerializable> StoreRefresh(IEnumerable<Guid> ids, IRepositoryService repos)
        {
            // TODO: we must check deleted ids - e.g. if a Class was removed, we must remove ClassTeacher, reference in School, Students etc...
            // 
            //var deletedXXX = new List<Class>();
            //deletedXXX.ForEach(o =>
            //{
            //    o.School.Classes.Remove(o);
            //});

            var serialized = serializedRepository.Get(ids);
            var deleted = ids.Except(serialized.Select(o => o.Id)).ToList();
            deleted.ForEach(o => values.Remove(o));

            foreach (var item in serialized)
            {
                var bo = item.ToBusinessObject(repos);
                values.Add(bo.Id, bo);
            }


            return serialized;
        }

        public void Remove(TBusinessClass item)
        {
            values.Remove(item.Id);
            serializedRepository.Remove(item.Id);
        }

        public IEnumerable<TBusinessClass> Query() => values.Values;
    }
}
