using Application.Core;
using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.Data.SqlClient;
using Persistence;

namespace Application.Activities
{
    public class List
    {
        public class Query : IRequest<Result<PagedList<ActivityDto>>>
        {
            public ActivityParams Params { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<PagedList<ActivityDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;
            private readonly IUserAccessor _userAccessor;
            public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor)
            {
                _userAccessor = userAccessor;
                _mapper = mapper;
                _context = context;
            }

            public async Task<Result<PagedList<ActivityDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var query = _context.Activities
                    .Where(d => d.Date >= request.Params.StartDate)
                    .OrderBy(d => d.Date)
                    .ProjectTo<ActivityDto>(_mapper.ConfigurationProvider,
                        new { currentUsername = _userAccessor.GetUsername() })
                    .AsQueryable();

                if (request.Params.IsGoing && !request.Params.IsHost)
                {
                    query = query.Where(x => x.Attendees.Any(a => a.Username == _userAccessor.GetUsername()));
                }

                if (request.Params.IsHost && !request.Params.IsGoing)
                {
                    query = query.Where(x => x.HostUsername == _userAccessor.GetUsername());
                }

                // search by title
                if (!string.IsNullOrWhiteSpace(request.Params.Search)){
                    query = query.Where(x => x.Title.Contains(request.Params.Search));
                }

                // sort
                query = GetSortQuery(query, request.Params.Sort ?? request.Params.DefaultSort);

                return Result<PagedList<ActivityDto>>.Success(
                    await PagedList<ActivityDto>.CreateAsync(query, request.Params.PageNumber,
                        request.Params.PageSize)
                );
            }


            // get sort query
            public IQueryable<ActivityDto> GetSortQuery(IQueryable<ActivityDto> query, string sort)
            {
                switch (sort)
                {
                    case "title":
                        query = query.OrderBy(s => s.Title);
                        break;
                    case "title_desc":
                        query = query.OrderByDescending(s => s.Title);
                        break;
                    case "date":
                        query = query.OrderBy(s => s.Date);
                        break;
                    case "date_desc":
                        query = query.OrderByDescending(s => s.Date);
                        break;
                    case "city":
                        query = query.OrderBy(s => s.City);
                        break;
                    case "city_desc":
                        query = query.OrderByDescending(s => s.City);
                        break;
                    case "venue":
                        query = query.OrderBy(s => s.Venue);
                        break;
                    case "venue_desc":
                        query = query.OrderByDescending(s => s.Venue);
                        break;
                    default:
                        query = query.OrderBy(s => s.Title);
                        break;
                }

                return query;
            }
        }
    }
}