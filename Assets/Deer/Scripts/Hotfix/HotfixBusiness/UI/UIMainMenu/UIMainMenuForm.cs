// ================================================
//描 述:
//作 者:Xiaohei.Wang(Wenhao)
//创建时间:2023-05-31 18-05-45
//修改作者:Xiaohei.Wang(Wenhao)
//修改时间:2023-05-31 18-05-45
//版 本:0.1 
// ===============================================

using HotfixFramework.Runtime;
using System.Collections;
using System.Collections.Generic;
using HotfixBusiness.Procedure;
using Main.Runtime;
using Main.Runtime.Procedure;
using UnityEngine;
using UnityGameFramework.Runtime;
using UnityEngine.Networking;
using System;

namespace HotfixBusiness.UI
{
	/// <summary>
	/// Please modify the description.
	/// </summary>
	public partial class UIMainMenuForm : UIFixBaseForm
	{
		private int connectCount = 0;
		private readonly int maxReconnectCount = 3;

		protected override void OnInit(object userData)
		{
			base.OnInit(userData);
			GetBindComponents(gameObject);

			/*--------------------Auto generate start button listener.Do not modify!--------------------*/
			m_Btn_DeerExample.onClick.AddListener(Btn_DeerExampleEvent);
			m_Btn_DeerGame.onClick.AddListener(Btn_DeerGameEvent);
			/*--------------------Auto generate end button listener.Do not modify!----------------------*/
		}

		private void Btn_DeerExampleEvent()
		{
			// if (!DeerSettingsUtils.DeerGlobalSettings.m_UseDeerExample)
			// {
			// 	DialogParams dialogParams = new DialogParams();
			// 	dialogParams.Mode = 1;
			// 	dialogParams.Title = "提示";
			// 	dialogParams.Message = "Deer例子已经被移除! [DeerTools/DeerExample/AddExample]可以添加Deer例子。";
			// 	dialogParams.ConfirmText = "确定";
			// 	GameEntry.UI.OpenDialog(dialogParams);
			// 	return;
			// }
			// if (GameEntry.Procedure.CurrentProcedure is ProcedureBase procedureBase)
			// {
			// 	procedureBase.ProcedureOwner.SetData<VarString>("nextProcedure", Constant.Procedure.ProcedureADeerExample);
			// 	procedureBase.ChangeStateByType(procedureBase.ProcedureOwner, typeof(ProcedureCheckAssets));
			// }
			StartCoroutine(IRequestSeverList());
		}

		private void Btn_DeerGameEvent()
		{
			// if (!DeerSettingsUtils.DeerGlobalSettings.m_UseDeerExample)
			// {
			// 	DialogParams dialogParams = new DialogParams();
			// 	dialogParams.Mode = 1;
			// 	dialogParams.Title = "提示";
			// 	dialogParams.Message = "Deer游戏例子已经被移除! [DeerTools/DeerExample/AddExample]可以添加Deer游戏例子。";
			// 	dialogParams.ConfirmText = "确定";
			// 	GameEntry.UI.OpenDialog(dialogParams);
			// 	return;
			// }
			// if (GameEntry.Procedure.CurrentProcedure is ProcedureBase procedureBase)
			// {
			// 	procedureBase.ProcedureOwner.SetData<VarString>("nextProcedure", Constant.Procedure.ProcedureAGameExample);
			// 	procedureBase.ChangeStateByType(procedureBase.ProcedureOwner, typeof(ProcedureCheckAssets));
			// }
			StartCoroutine(IRequestSeverList());
		}

		private IEnumerator IRequestSeverList()
		{
			connectCount = 0;
			bool connectFinish = false;

			while (!connectFinish)
			{
				using UnityWebRequest webRequest = UnityWebRequest.Get("https://rpg-wx.fjbgwl.com:35002/");
				yield return webRequest.SendWebRequest();
				if (!string.IsNullOrEmpty(webRequest.error))
				{
					Log.Error("获取服务器列表失败 error = " + webRequest.error);
					connectCount++;
					if (connectCount >= maxReconnectCount)
					{
						connectFinish = true;
					}
					yield return new WaitForSeconds(0.5f);
				}
				else
				{
					connectFinish = true;
					Servers servers = JsonUtility.FromJson<Servers>(webRequest.downloadHandler.text);
					// serverList = servers.gateServers;

					if (servers.gateServers.Count > 0)
					{
						GameEntry.WebSocket.ConnectNet(servers.gateServers[0].ip, servers.gateServers[0].port);
					}
				}
			}
		}
		/*--------------------Auto generate footer.Do not add anything below the footer!------------*/
	}
}

[Serializable]
public class ServerInfo
{
	public int cellId;
	public int id;
	public string ip;
	public string name;
	public int port;
}

[Serializable]
public class Servers
{
	public List<ServerInfo> gateServers;
}
