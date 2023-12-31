﻿using System;
using Genesyslab.Desktop.Modules.Voice.Model.Agents;

namespace Genesyslab.Desktop.Modules.Incom.UserEvenManagment
{
    /// <summary>
    /// Interface definition for UserEventListener.
    /// </summary>
    public interface IUserEventListener
    {
            String AgentDN { set; get; }
             IMediaVoice MediaVoice { get; set; }
            void SetupHandler();
            void RemoveHandler();
    }
}
