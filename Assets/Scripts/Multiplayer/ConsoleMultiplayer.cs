// using UnityEngine;
// using System.Collections.Generic;
// using System;
// using TheIslandNetworking;

// public class ConsoleMultiplayer : MonoBehaviour
// {
//     private const string SEND = "player";
//     private void Start()
//     {
//         Console.AddExeption(SEND, Send);
//     }
//     private void Send(List<string> Command, Dictionary<string, Action<List<string>>> commandList)
//     {
//         if (Command.Count < 3)
//             return;
//         ushort playerId = ushort.Parse(Command[1]);
//         Command.RemoveAt(0);
//         Command.RemoveAt(0);
//         SendCommand(Command, playerId);
//     }
//     void SendCommand(List<string> Command, ushort id)
//     {
//         // Send Command to specific Client
//         Message commandMessage = Message.Create(MessageSendMode.reliable, MessageId.commands);
//         if (NetworkManager.networkMode == NetworkMode.Client)
//             commandMessage.AddUShort(id);
//         commandMessage.AddUShort((ushort)Command.Count);
//         foreach (string i in Command)
//             commandMessage.AddString(i);
        
//         if (NetworkManager.networkMode == NetworkMode.Host)
//             serverHandler.Instance.SendSpecific(commandMessage, id);
//         else
//             NetworkManager.Send(commandMessage);
//     }
//     [MessageHandler((ushort)MessageId.commands)]
//     public static void ReceiveCommand(Message message)
//     {
//         int count = message.GetUShort();
//         List<string> Command = new List<string>();
//         for (int i = 0; i < count; i++)
//             Command.Add(message.GetString());
//         Console.Instance.Execute(Command);
//     }
// }
