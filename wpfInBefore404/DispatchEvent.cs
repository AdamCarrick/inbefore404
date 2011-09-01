using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Threading;

namespace wpfInBefore404
{
  public class DispatchEvent
  {
    private List<DispatchHandler> handlerList = new List<DispatchHandler>();

    public void Add(Delegate handler)
    {
      this.Add(handler, Dispatcher.CurrentDispatcher);
    }

    public void Add(Delegate handler, Dispatcher dispatcher)
    {
      this.handlerList.Add(new DispatchHandler(handler, dispatcher));
    }

    public void Clear()
    {
      foreach (DispatchHandler handler in this.handlerList) {
        handler.Dispose();
      }
      this.handlerList.Clear();
    }

    public void Fire(object sender, EventArgs args)
    {
      foreach (DispatchHandler handler in (from handler in this.handlerList
                                           where handler.IsDisposable
                                           select handler).ToArray<DispatchHandler>()) {
        this.handlerList.Remove(handler);
        handler.Dispose();
      }
      foreach (DispatchHandler handler2 in this.handlerList) {
        handler2.Invoke(sender, new object[] { args });
      }
    }

    public void Remove(Delegate handler)
    {
      DispatchHandler[] handlerArray = (from dispatchHandler in this.handlerList
                                        where dispatchHandler.DelegateEquals(handler)
                                        select dispatchHandler).ToArray<DispatchHandler>();
      if ((handlerArray != null) && (handlerArray.Length > 0)) {
        this.handlerList.Remove(handlerArray[0]);
        handlerArray[0].Dispose();
      }
    }

    private class DispatchHandler : IDisposable
    {
      private WeakReference dispatcherRef;
      private MethodInfo handlerInfo;
      private WeakReference targetRef;

      public DispatchHandler(Delegate handler, System.Windows.Threading.Dispatcher dispatcher)
      {
        this.handlerInfo = handler.Method;
        this.targetRef = new WeakReference(handler.Target);
        this.dispatcherRef = new WeakReference(dispatcher);
      }

      public bool DelegateEquals(Delegate other)
      {
        object target = this.Target;
        return (((target != null) && object.ReferenceEquals(target, other.Target)) && (this.handlerInfo.Name == other.Method.Name));
      }

      public void Dispose()
      {
        this.targetRef = null;
        this.handlerInfo = null;
        this.dispatcherRef = null;
      }

      public void Invoke(object arg, params object[] args)
      {
        EventHandler method = null;
        EventHandler handler2 = null;
        object target = this.Target;
        System.Windows.Threading.Dispatcher dispatcher = this.Dispatcher;
        if (!this.IsDisposable) {
          if (this.IsDispatcherThreadAlive) {
            if (method == null) {
              method = delegate(object sender, EventArgs e)
              {
                this.handlerInfo.Invoke(target, new object[] { arg, e });
              };
            }
            dispatcher.Invoke(DispatcherPriority.Send, method, arg, args);
          } else if (target is DispatcherObject) {
            if (handler2 == null) {
              handler2 = delegate(object sender, EventArgs e)
              {
                this.handlerInfo.Invoke(target, new object[] { arg, e });
              };
            }
            dispatcher.BeginInvoke(DispatcherPriority.Send, handler2, arg, args);
          } else {
            ArrayList list = new ArrayList();
            list.Add(arg);
            list.AddRange(args);
            this.handlerInfo.Invoke(target, list.ToArray());
          }
        }
      }

      private System.Windows.Threading.Dispatcher Dispatcher
      {
        get
        {
          return (System.Windows.Threading.Dispatcher)this.dispatcherRef.Target;
        }
      }

      private bool IsDispatcherThreadAlive
      {
        get
        {
          return this.Dispatcher.Thread.IsAlive;
        }
      }

      public bool IsDisposable
      {
        get
        {
          object target = this.Target;
          System.Windows.Threading.Dispatcher dispatcher = this.Dispatcher;
          return (((target == null) || (dispatcher == null)) || ((target is DispatcherObject) && ((dispatcher.Thread.ThreadState & (ThreadState.Aborted | ThreadState.AbortRequested | ThreadState.Stopped | ThreadState.StopRequested)) != ThreadState.Running)));
        }
      }

      private object Target
      {
        get
        {
          return this.targetRef.Target;
        }
      }
    }
  }
}