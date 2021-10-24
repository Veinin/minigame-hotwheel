using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Game.Core.Events
{
    public class Message
    {
        public string type;
        public Message() { type = this.GetType().Name; }
    }

    public delegate void MessageHandlerDelegate(Message message);

    public class EventMnager : SingletonObject<EventMnager>
    {
        private const int MaxQueueProcessingTime = 16667;

        private Stopwatch m_timer = new Stopwatch();
        private Queue<Message> m_messageQueue = new Queue<Message>();
        private Dictionary<string, List<MessageHandlerDelegate>> m_listenerDict = new Dictionary<string, List<MessageHandlerDelegate>>();

        public bool AddListener<T>(MessageHandlerDelegate handler) where T : Message
        {
            string messageType = typeof(T).Name;
            if (!m_listenerDict.ContainsKey(messageType))
            {
                m_listenerDict.Add(messageType, new List<MessageHandlerDelegate>());
            }

            List<MessageHandlerDelegate> listenerList = m_listenerDict[messageType];
            if (listenerList.Contains(handler))
            {
                return false;
            }

            listenerList.Add(handler);
            return true;
        }

        public bool RemoveListener<T>(MessageHandlerDelegate handler)
        {
            string messageType = typeof(T).Name;
            if (!m_listenerDict.ContainsKey(messageType))
            {
                return false;
            }

            var listenerList = m_listenerDict[messageType];
            if (!listenerList.Contains(handler))
            {
                return false;
            }

            listenerList.Remove(handler);
            return true;
        }

        public bool EnqueueMessage(Message msg)
        {
            if (!m_listenerDict.ContainsKey(msg.type))
            {
                return false;
            }

            m_messageQueue.Enqueue(msg);
            return true;
        }

        public bool EmitMessage(Message msg)
        {
            string msgType = msg.type;
            if (!m_listenerDict.ContainsKey(msgType))
            {
                Debug.Log("EventManager:Message \"" + msgType + "\" has no listeners!");
                return false;
            }

            var listenerList = m_listenerDict[msgType];

            for (int i = 0; i < listenerList.Count; ++i)
            {
                listenerList[i](msg);
            }
            return true;
        }

        void Update()
        {
            m_timer.Start();

            while (m_messageQueue.Count > 0)
            {
                if (MaxQueueProcessingTime > 0.0f)
                {
                    if (m_timer.Elapsed.Milliseconds > MaxQueueProcessingTime)
                    {
                        m_timer.Stop();
                        return;
                    }
                }

                Message msg = m_messageQueue.Dequeue();
                if (!EmitMessage(msg))
                {
                    Debug.Log("Error when processing message: " + msg.type);
                }
            }
        }
    }
}