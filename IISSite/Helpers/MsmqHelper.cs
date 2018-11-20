using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Messaging;
using System.Net;
using Microsoft.Win32;

namespace IISSite.Helpers
{
    public class MsmqHelper
    {
        public static string GetDirectFormatName(string qnamePath)
        {
            return string.Format("FormatName:DIRECT=OS:{0}", qnamePath);
        }

        public static string GetDirectFormatName(string machineName, string qname, bool privateQueue)
        {
            string qPath = string.Empty;
            bool UseTcp = false;
            bool useHttp = false;

            //If the machine name is the IP address, need to use TCP as the protocol
            try
            {
                IPAddress address = IPAddress.Parse(machineName);
                UseTcp = true;
            }
            catch
            {
                UseTcp = false;
            }

            if (machineName.ToLower().StartsWith("http"))
            {
                useHttp = true;
            }

            if (UseTcp)
            {
                qPath = (privateQueue) ? string.Format("FormatName:DIRECT=TCP:{0}\\private$\\{1}", machineName, qname) : string.Format("FormatName:DIRECT=TCP:{0}\\{1}", machineName, qname);
            }
            else if (useHttp)
            {
                qPath = $"FormatName:DIRECT={machineName}/msmq/{qname}";
            }
            else
            {
                qPath = (privateQueue) ? string.Format("FormatName:DIRECT=OS:{0}\\private$\\{1}", machineName, qname) : string.Format("FormatName:DIRECT=OS:{0}\\{1}", machineName, qname);
            }
            return qPath;
        }

        public static string GetQueueName(string machineName, string qname, bool privateQueue)
        {
            return (privateQueue) ? string.Format("{0}\\private$\\{1}", machineName, qname) : string.Format("{0}\\{1}", machineName, qname);
        }
        public static MessageQueue GetOrCreateQueue(string qName, string qMachineName, bool isPrivate, bool forceUseDirectName = false, bool MustAllowSend = true, bool MustAllowRead = true)
        {
            MessageQueue mq = null;
            string directQueueName = string.Empty;
            string friendlyQueueName = string.Empty;
            bool canRead = false;
            bool canCreate = false;
            bool canDelete = false;
            bool canPeek = false;
            bool canPurge = false;
            bool canRecieve = false;
            bool canGetProps = false;
            bool canGetPerms = false;
            bool canSend = false;
            bool isLocalQ = IsQueueLocal(qMachineName);
            bool isUsingHttp = qMachineName.ToLower().StartsWith("http");

            bool.TryParse(Registry.LocalMachine.GetValue(@"SOFTWARE\Microsoft\MSMQ\Parameters\Workgroup")?.ToString(), out bool isWorkgroupMode);
            bool.TryParse(Registry.LocalMachine.GetValue(@"SOFTWARE\Microsoft\MSMQ\Parameters\Setup\AlwaysWithoutDS")?.ToString(), out bool alwaysDS);

            //If the queue is remote, need to see how to create the correct name
            if (forceUseDirectName)
            {
                directQueueName = MsmqHelper.GetDirectFormatName(qMachineName, qName, isPrivate);
                friendlyQueueName = directQueueName;
                Console.WriteLine($"GetOrCreateQueue::Force Using the direct name {directQueueName}");
            }
            else
            {
                if ((isWorkgroupMode || alwaysDS || !isLocalQ))
                {
                    directQueueName = MsmqHelper.GetDirectFormatName(qMachineName, qName, isPrivate);
                    friendlyQueueName = MsmqHelper.GetQueueName(qMachineName, qName, isPrivate);
                    Console.WriteLine($"GetOrCreateQueue::Remote queue and not in domain mode, use the direct name {directQueueName}");
                }
                else
                {
                    directQueueName = MsmqHelper.GetQueueName(qMachineName, qName, isPrivate);
                    friendlyQueueName = directQueueName;
                    Console.WriteLine($"GetOrCreateQueue::Remote or local queue and in domain mode, use the name {directQueueName}");
                }
            }

            //Check the current mode to see how to send messages
            //Public format names cannot be used on computers operating in workgroup mode.
            //Private format names cannot be used on computers operating in workgroup mode.
            //In workgroup mode, you can use direct format names to open any 
            //  public or private queue for sending messages, or to open local and remote private queues for reading messages.

            Console.WriteLine($"GetOrCreateQueue::Trying to get queue {directQueueName}");

            //Try to get the queue and see what we can do with it
            if (!isUsingHttp)
            {
                try
                {
                    //See if we can read. Use the direct name
                    Console.WriteLine($"GetOrCreateQueue::Try to read the queue using format name {directQueueName}");
                    mq = new MessageQueue(directQueueName, QueueAccessMode.Peek);
                    canRead = mq.CanRead;
                }
                catch (Exception rex)
                {
                    Console.WriteLine($"GetOrCreateQueue::CanReadException {directQueueName}::{rex.ToString()}");

                }

                try
                {
                    //See if we can send. Use the direct name
                    Console.WriteLine($"GetOrCreateQueue::Try to send a message to the queue using format name {directQueueName}");
                    mq = new MessageQueue(directQueueName, QueueAccessMode.Send);
                    canSend = mq.CanWrite;
                }
                catch (Exception sendex)
                {
                    Console.WriteLine($"GetOrCreateQueue::CanSendException {directQueueName}::{sendex.ToString()}");
                }
            }
            else
            {
                //Using HTTP we can look to see if we can send or receive. Just say yes
                canRead = true;
                canSend = true;
            }


            try
            {
                Console.WriteLine($"GetOrCreateQueue::Get the queue using format name {directQueueName}");
                QueueAccessMode queueAccessMode;

                if (MustAllowRead && canRead && MustAllowSend && canSend)
                {
                    queueAccessMode = QueueAccessMode.SendAndReceive;
                }
                else if (MustAllowRead && canRead)
                {
                    queueAccessMode = QueueAccessMode.Receive;
                }
                else if (MustAllowSend && canSend)
                {
                    queueAccessMode = QueueAccessMode.Send;
                }
                else
                {
                    throw new Exception($"GetOrCreateQueue::Queue access requested is not allowed CanRead:{canRead} CanSend:{canSend}");
                }

                mq = new MessageQueue(directQueueName, queueAccessMode)
                {
                    Formatter = new BinaryMessageFormatter()
                };
            }
            catch (Exception ex)
            {
                throw new ArgumentOutOfRangeException($"GetOrCreateQueue::Could not get the queue using format name:[{qName}] Path:[{directQueueName}]::{ex.ToString()}");
            }

            return mq;
        }

        public static bool IsQueueLocal(string qMachineName)
        {
            string localMachineName = Environment.MachineName;
            return (qMachineName == ".") || (string.Compare(qMachineName, localMachineName, true) == 0);
        }

        public static bool IsQueueAvailable(string queueName)
        {
            //In workgroup mode, you can use direct format names to open 
            // any public or private queue for sending messages, or to open local and remote private queues for reading messages.

            MessageQueue queue;
            bool retVal = false; //Assume not available
            try
            {
                Console.WriteLine($"IsQueueAvailable::Peeking {queueName}");
                queue = new MessageQueue(queueName, QueueAccessMode.Peek);
                if (queue.CanRead)
                {
                    if (queue.GetAllMessages().Length > 0)
                    {
                        Message msg = queue.Peek(new TimeSpan(0, 0, 5)); // wait max. 5 sec. to recieve first message from queue (reduce if necessary)
                        if (msg != null)
                        {
                            //We can peek the queue
                            retVal = true;
                        }
                    }
                    else
                    {
                        retVal = true;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is ArgumentException)
                {   // the provided queue name is wrong.
                    Console.WriteLine($"IsQueueAvailable::Check the qname {queueName}::{ex.ToString()}");
                    retVal = false;
                }
                else if (ex is MessageQueueException)
                {   // if message queue exception occurs either the queue is avialable but without entries (check for peek timeout) or the queue does not exist or you don't have access.
                    Console.WriteLine($"IsQueueAvailable::MessageQueueException {queueName}::{ex.ToString()}");
                    Console.WriteLine($"IsQueueAvailable::MessageQueueErrorCode {queueName}::{((MessageQueueException)ex).MessageQueueErrorCode}");
                    //If its a timeout, than it could be that we didnt have any messages. Anything else is a real error
                    retVal = (((MessageQueueException)ex).MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout);
                }
                else
                {
                    // any other error occurred.
                    Console.WriteLine($"IsQueueAvailable::GeneralException {queueName}::{ex.ToString()}");
                    retVal = false;
                }
            }
            return retVal;
        }
    }
}