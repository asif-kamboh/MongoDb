namespace ScientificBit.MongoDb.Views;

/// <summary>
/// View for paged listing
/// </summary>
/// <typeparam name="TDocument"></typeparam>
public interface IListViewModel<TDocument>
{
	/// <summary>
	/// List of documents
	/// </summary>
	IList<TDocument> Documents { get; }

	/// <summary>
	/// offset
	/// </summary>
	int Offset { get; }

	/// <summary>
	/// Page size
	/// </summary>
	int Limit { get; }

	/// <summary>
	/// Currently not used
	/// </summary>
	int Total { get; }
}

/// <summary>
/// Base class implementation of Paged list view model
/// </summary>
/// <typeparam name="TDocument"></typeparam>
public class ListViewModel<TDocument> : IListViewModel<TDocument>
{
	public ListViewModel() : this(new List<TDocument>())
	{
	}

	public ListViewModel(IList<TDocument>? documents) : this(documents, 0, documents?.Count ?? 0)
	{
	}

	public ListViewModel(IList<TDocument>? documents, int limit) : this(documents, 0, limit)
	{
	}

	public ListViewModel(IList<TDocument>? documents, int offset, int limit) : this(documents, offset, limit, documents?.Count ?? 0)
	{
	}

	public ListViewModel(IList<TDocument>? documents, int offset, int limit, int total)
	{
		Documents = documents?.ToList() ?? new List<TDocument>();
		Offset = offset;
		Limit = limit;
		Total = total;
	}

    public IList<TDocument> Documents { get; set; }

	public int Offset { get; set; }

	public int Limit { get; set; }

	public int Total { get; set; }
}