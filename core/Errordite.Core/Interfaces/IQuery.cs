﻿namespace Errordite.Core.Interfaces
{
    public interface IQuery<in TRequest, out TResponse> : IWorkflow<TRequest, TResponse>
    { }
}