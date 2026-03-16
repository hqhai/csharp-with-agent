// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Domain.Entities;
    using global::System.Collections.Generic;
    using global::System.Text.Json;

    public class DictionaryAIProfile : Profile
    {
        public DictionaryAIProfile()
        {
            CreateMap<DictionaryAI, SemanticDictionaryResultModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.Notes, opt => opt.Ignore())
                .ForMember(dest => dest.SimilarityScore, opt => opt.MapFrom(src => src.Similarity))
                .ForMember(dest => dest.IsFromCache, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    // Parse NotesJson to List<SemanticNoteModel>
                    if (!string.IsNullOrEmpty(src.NotesJson))
                    {
                        try
                        {
                            dest.Notes = JsonSerializer.Deserialize<List<SemanticNoteModel>>(src.NotesJson, new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });
                        }
                        catch
                        {
                            dest.Notes = new List<SemanticNoteModel>();
                        }
                    }
                    else
                    {
                        dest.Notes = new List<SemanticNoteModel>();
                    }

                    // Fallback example sentences from JsonPayload if not set
                    if (!string.IsNullOrEmpty(src.JsonPayload))
                    {
                        try
                        {
                            var payload = JsonSerializer.Deserialize<SemanticDictionaryResultModel>(src.JsonPayload, new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });
                            if (payload != null)
                            {
                                dest.ExampleSentenceSource ??= payload.ExampleSentenceSource;
                                dest.ExampleSentenceTarget ??= payload.ExampleSentenceTarget;
                            }
                        }
                        catch
                        {
                            // Ignore payload parse errors
                        }
                    }
                })
                .IgnoreAllNonExisting();
        }
    }
}
