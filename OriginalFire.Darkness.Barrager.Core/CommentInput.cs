using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Threading;

namespace OriginalFire.Darkness.Barrager.Core
{
	/// <summary>
	/// 为弹幕输入源提供抽象基类。
	/// </summary>
	public abstract class CommentInput : IDisposable
	{
		/// <summary>
		/// 初始化CommentInput类的新实例。
		/// </summary>
		protected CommentInput()
		{
			operation = AsyncOperationManager.CreateOperation(Guid.NewGuid());
			onComment = new SendOrPostCallback(o =>
				{
					if (Comment != null)
						Comment(this, new CommentEventArgs((string)o));
				}
			);
		}

		/// <summary>
		/// 根据设置项进行弹幕源的初始化，并开始接收弹幕评论。
		/// </summary>
		/// <param name="configurations">包含CommentInput相关设置的设置项集合。</param>
		public virtual void Initialize(Configurations configurations)
		{
		}

		/// <summary>
		/// 释放此CommentInput所持有的资源。
		/// </summary>
		public virtual void Dispose()
		{
		}

		/// <summary>
		/// 有新弹幕要产生时发生。
		/// </summary>
		public event CommentEventHandler Comment;

		/// <summary>
		/// 添加弹幕。
		/// </summary>
		/// <param name="comment">弹幕将要包含的评论内容。</param>
		protected void SubmitComment(string comment)
		{
			if (comment.Length == 0)
				return;
			operation.Post(onComment, comment);
		}

		private SendOrPostCallback onComment;

		private AsyncOperation operation;
	}
}
