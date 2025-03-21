using Application.Common.CQS.Queries;
using Application.Common.Repositories;
using Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace Application.Features.DataManager;
    
public class ResetDataResult
{

}

public class ResetDataRequest : IRequest<ResetDataResult>
{

}

public class ResetDataHandler : IRequestHandler<ResetDataRequest, ResetDataResult>
{
    private readonly ICommandRepository<BaseEntity> _commandRepository;

    public ResetDataHandler(ICommandRepository<BaseEntity> commandRepository)
    {
        _commandRepository = commandRepository;
    }

    public async Task<ResetDataResult> Handle(ResetDataRequest request, CancellationToken cancellationToken)
    {
        await _commandRepository.ClearDatabase(cancellationToken);
        return new ResetDataResult();
    }
}
