using DocFinder.Application.Commands;
using DocFinder.Domain;
using DocFinder.Services;

namespace DocFinder.Application.Handlers;

public sealed class AddDataHandler : AddEntityHandler<Data>
{
    public AddDataHandler(DataService service) : base(service) { }
}
