using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OriginalFire.Darkness.Barrager.Core
{
	/// <summary>
	/// 表示将要处理Comment事件。
	/// </summary>
	/// <param name="sender">事件源。</param>
	/// <param name="e">包含Comment事件的事件数据。</param>
	public delegate void CommentEventHandler(object sender, CommentEventArgs e);
}
