﻿using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdate
{
    public class Application : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            application.ControlledApplication.DocumentOpened -= application_DocumentOpened;
            CloseLogFile();
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                // Create a temp file to dump the log, and copy this file to log file 
                // at the end of the sample.
                CreateTempFile();

                // Register event. In this sample, we trigger this event from UI, so it must 
                // be registered on ControlledApplication. 
                application.ControlledApplication.DocumentOpened += new EventHandler
                    <DocumentOpenedEventArgs>(application_DocumentOpened);
            }
            catch (Exception)
            {
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        private void application_DocumentOpened(object sender, DocumentOpenedEventArgs args)
        {
            // dump the args to log file.
            DumpEventArgs(args);

            // get document from event args.
            Document doc = args.Document;

            if (doc.IsFamilyDocument)
            {
                return;
            }
            try
            {
                //now event framework will not provide transaction,user need start by self(2009/11/18)
                Transaction eventTransaction = new Transaction(doc, "Event handler modify project information");
                eventTransaction.Start();
                // assign specified value to ProjectInformation.Address property. 
                // User can change another properties of document or create(delete) something as he likes.
                // Please pay attention that ProjectInformation property is empty for family document.
                // But it isn't the correct usage. So please don't run this sample on family document.
                doc.ProjectInformation.Address =
                    "United States - Massachusetts - Waltham - 610 Lincoln St";
                eventTransaction.Commit();
            }
            catch (Exception ee)
            {
                Trace.WriteLine("Failed to modify project information!-" + ee.Message);
            }
            // write the value to log file to check whether the operation is successful.
            Trace.WriteLine("The value after running the sample ------>");
            Trace.WriteLine("    [Address]         :" + doc.ProjectInformation.Address);
        }

        private void DumpEventArgs(DocumentOpenedEventArgs args)
        {
            Trace.WriteLine("DocumentOpenedEventArgs Parameters ------>");
            Trace.WriteLine("    Event Cancel      : " + args.IsCancelled()); // is it be cancelled?
            Trace.WriteLine("    Event Cancellable : " + args.Cancellable); // Cancellable
            Trace.WriteLine("    Status            : " + args.Status); // Status
        }

        /// <summary>
        /// Create a log file to track the subscribed events' work process.
        /// </summary>
        private void CreateTempFile()
        {
            if (File.Exists(m_tempFile)) File.Delete(m_tempFile);
            m_txtListener = new TextWriterTraceListener(m_tempFile);
            Trace.AutoFlush = true;
            Trace.Listeners.Add(m_txtListener);
        }

        /// <summary>
        /// Close the log file and remove the corresponding listener.
        /// </summary>
        private void CloseLogFile()
        {
            // close listeners now.
            Trace.Flush();
            Trace.Listeners.Remove(m_txtListener);
            Trace.Close();
            m_txtListener.Close();

            // copy temp file to log file and delete the temp file.
            String logFile = Path.Combine(m_directory, "AutoUpdate.log");
            if (File.Exists(logFile)) File.Delete(logFile);
            File.Copy(m_tempFile, logFile);
            File.Delete(m_tempFile);
        }

        TextWriterTraceListener m_txtListener;
        static String m_directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        String m_tempFile = Path.Combine(m_directory, "temp.log");
    }
}
