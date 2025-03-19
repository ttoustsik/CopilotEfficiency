using AutoMapper;
using CopilotEfficiency.Business.Models;
using CopilotEfficiency.Core.Entities;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Octokit;

namespace CopilotEfficiency.Business.Mappings;

public class PullRequestMappingProfile : Profile
{
    public PullRequestMappingProfile()
    {

        CreateMap<DateTimeOffset, DateTime>()
            .ConvertUsing(dto => dto.UtcDateTime);

        CreateMap<PullRequest, RepositoryPullRequestDto>()
            .ForMember(dest => dest.RepositoryId, opt => opt.MapFrom(src => src.Base.Repository.Id))
            .ForMember(dest => dest.RepositoryName, opt => opt.MapFrom(src => src.Base.Repository.FullName))
            .ForMember(dest => dest.RepositoryUrl, opt => opt.MapFrom(src => src.Base.Repository.Url))
            .ForMember(dest => dest.JsonUrl, opt => opt.MapFrom(src => src.Url))
            .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.Base.Repository.Id))
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Base.Repository.FullName))
            .ForMember(dest => dest.PullRequestId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.PullRequestStatus, opt => opt.MapFrom(src => src.State))
            .ForMember(dest => dest.PullRequestTitle, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => src.User.Id))
            .ForMember(dest => dest.AuthorDisplayName, opt => opt.MapFrom(src => src.User.Login))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.DateTime))
            .ForMember(dest => dest.ClosedAt, opt => opt.MapFrom(src => src.ClosedAt.HasValue ? src.ClosedAt.Value.UtcDateTime : DateTime.MinValue))
            .ForMember(dest => dest.ClosedById, opt => opt.MapFrom(src => src.MergedBy.Id));

        CreateMap<GitPullRequest, RepositoryPullRequestDto>()
            .ForMember(dest => dest.RepositoryId, opt => opt.MapFrom(src => src.Repository.Id))
            .ForMember(dest => dest.RepositoryName, opt => opt.MapFrom(src => src.Repository.Name))
            .ForMember(dest => dest.RepositoryUrl, opt => opt.MapFrom(src => src.Repository.RemoteUrl))
            .ForMember(dest => dest.JsonUrl, opt => opt.MapFrom(src => src.Url))
            .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.Repository.ProjectReference.Id))
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Repository.ProjectReference.Name))
            .ForMember(dest => dest.PullRequestId, opt => opt.MapFrom(src => src.PullRequestId))
            .ForMember(dest => dest.PullRequestStatus, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.PullRequestTitle, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => src.CreatedBy.Id))
            .ForMember(dest => dest.AuthorDisplayName, opt => opt.MapFrom(src => src.CreatedBy.UniqueName))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreationDate))
            .ForMember(dest => dest.ClosedAt, opt => opt.MapFrom(src => src.ClosedDate))
            .ForMember(dest => dest.ClosedById, opt => opt.MapFrom(src => src.ClosedBy.Id))
            .ForMember(dest => dest.ClosedByDisplayName, opt => opt.MapFrom(src => src.ClosedBy.UniqueName));

        CreateMap<RepositoryPullRequestDto, RepositoryPullRequestEntity>();
    }
}
