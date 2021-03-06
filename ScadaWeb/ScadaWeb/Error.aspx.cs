﻿/*
 * Copyright 2016 Mikhail Shiryaev
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * 
 * Product  : Rapid SCADA
 * Module   : SCADA-Web
 * Summary  : Error message web form
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2007
 * Modified : 2016
 */

using Scada.UI;
using System;
using System.Text;
using Utils;

namespace Scada.Web
{
    /// <summary>
    /// Error message web form
    /// <para>Веб-форма сообщения об ошибке приложения</para>
    /// </summary>
    public partial class WFrmError : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                // настройка выходного потока
                Response.ClearContent();
                Response.TrySkipIisCustomErrors = true;

                // перевод веб-страницы
                Translator.TranslatePage(this, "Scada.Web.WFrmError");

                // определение сообщения об ошибке
                string errMsg = "";
                Exception ex = Server.GetLastError();

                if (ex == null)
                {
                    string msg = Request["msg"];
                    if (!string.IsNullOrEmpty(msg))
                        errMsg = msg;
                }
                else
                {
                    if (ex.InnerException != null)
                        ex = ex.InnerException;
                    errMsg = ex.Message;
                }

                if (errMsg == "")
                {
                    errMsg = lblMessage.Text;
                }
                else
                {
                    errMsg = lblMessage.Text + ": " + errMsg;
                    lblMessage.Text = ScadaUtils.HtmlEncodeWithBreak(errMsg); // вывод на форму
                }

                // вывод сообщения с дополнительной информацией в журнал приложения
                StringBuilder sbLogMsg = new StringBuilder(errMsg);
                if (ex != null)
                    sbLogMsg.AppendLine().Append(ex.StackTrace);
                
                sbLogMsg
                    .AppendLine()
                    .Append(Localization.UseRussian ? "Страница: " : "Page: ")
                    .Append(Request.Url.AbsoluteUri);

                if (Context.Session == null)
                {
                    AppData.InitAppData();
                }
                else
                {
                    UserData userData = UserData.GetUserData();
                    if (userData != null)
                        sbLogMsg
                            .AppendLine()
                            .Append(Localization.UseRussian ? "Пользователь: " : "User: ")
                            .Append(userData.UserLogin);
                }

                AppData.Log.WriteAction(sbLogMsg.ToString(), Log.ActTypes.Exception);
            }
            finally
            {
                Server.ClearError();
            }
        }
    }
}