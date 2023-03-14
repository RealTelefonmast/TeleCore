﻿using System;
using System.IO;
using System.Threading;
using Verse;

namespace TeleCore;

public static class FileSaveUtility
{
    public static void ProcessSavingAction(string fullPath, string documentElementName, Action saveAction)
    {
        try
        {
            Scribe.saver.InitSaving(fullPath, documentElementName);
            saveAction();
            Scribe.saver.FinalizeSaving();
        }
        catch (Exception ex)
        {
            TLog.Warning($"An exception was thrown during saving to \"{fullPath}\": {ex}");
            Scribe.saver.ForceStop();
            SafeSaver.RemoveFileIfExists(fullPath, false);
            throw;
        }
    }

    public static void RemoveFileIfExists(string path, bool rethrow)
    {
        try
        {
            if (File.Exists(path)) File.Delete(path);
        }
        catch (Exception ex)
        {
            TLog.Warning($"Could not remove file \"{path}\": {ex}");
            if (rethrow) throw;
        }
    }

    public static void MoveFile(string from, string to)
    {
        Exception ex = null;
        for (int i = 0; i < 50; i++)
        {
            try
            {
                File.Move(from, to);
                return;
            }
            catch (Exception ex2)
            {
                if (ex == null)
                {
                    ex = ex2;
                }
            }

            Thread.Sleep(1);
        }

        throw ex;
    }
}
