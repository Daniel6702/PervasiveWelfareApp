﻿namespace WelfareMonitorApp.Services
{
    public interface IDialogService
    {
        Task ShowDialogAsync(string title, string message, string cancel);
    }
}