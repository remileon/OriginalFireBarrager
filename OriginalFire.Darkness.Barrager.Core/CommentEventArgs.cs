using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OriginalFire.Darkness.Barrager.Core
{
	/// <summary>
	/// 为弹幕产生事件提供事件数据。
	/// </summary>
	public class CommentEventArgs : EventArgs
	{
		/// <summary>
		/// 构造CommentEventArgs类的新实例。
		/// </summary>
		/// <param name="comment">该CommentEventArgs所包含的弹幕评论内容。</param>
		public CommentEventArgs(string comment)
		{
			this.comment = comment;
		}

		/// <summary>
		/// 获取该CommentEventArgs所包含的弹幕评论内容。
		/// </summary>
		public string Comment
		{
			get
			{
				return comment;
			}
		}

		private string comment;
	}
}
