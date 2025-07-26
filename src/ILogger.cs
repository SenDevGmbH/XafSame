#if !NET48
#endif
using System;

namespace SenDev.XafSame;
interface ILogger
{
    void LogError(Exception ex);
    void LogInfo(string message);
}