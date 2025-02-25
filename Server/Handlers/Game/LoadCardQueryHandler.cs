﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using nue.protocol.exvs;
using Serilog;
using Server.Persistence;
using System.Text.Json;
using Newtonsoft.Json;

namespace Server.Handlers.Game;

public record LoadCardQuery(Request Request) : IRequest<Response>;

public class LoadCardQueryHandler : IRequestHandler<LoadCardQuery, Response>
{
    private readonly ServerDbContext _context;

    public LoadCardQueryHandler(ServerDbContext context)
    {
        _context = context;
    }

    public Task<Response> Handle(LoadCardQuery query, CancellationToken cancellationToken)
    {
        var request = query.Request;

        var response = new Response
        {
            Type = request.Type,
            RequestId = request.RequestId,
            Error = Error.Success,
        };

        var sessionId = request.load_card.SessionId;
        
        var cardProfile = _context.CardProfiles
            .Include(x => x.PilotDomain)
            .Include(x => x.UserDomain)
            .FirstOrDefault(x => x.SessionId == sessionId);

        if (cardProfile == null)
        {
            response.Error = Error.ErrServer;
            return Task.FromResult(response);
        }
        
        response.load_card = new Response.LoadCard
        {
            pilot_data_group = JsonConvert.DeserializeObject<Response.LoadCard.PilotDataGroup>(cardProfile.PilotDomain.PilotDataGroupJson),
            mobile_user_group = JsonConvert.DeserializeObject<Response.LoadCard.MobileUserGroup>(cardProfile.UserDomain.MobileUserGroupJson),
        };

        //String readStr = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "loadcard.json"));
        //response.load_card = JsonConvert.DeserializeObject<Response.LoadCard>(loadCard);

        return Task.FromResult(response);
    }
}