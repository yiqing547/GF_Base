using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HotfixBusiness.UI
{
	public partial class UITask_Cell
	{
		private Image m_Img_BG;
		private TextMeshProUGUI m_TxtM_Title;

		private void GetBindComponents(GameObject go)
		{
			ComponentAutoBindTool autoBindTool = go.GetComponent<ComponentAutoBindTool>();

			m_Img_BG = autoBindTool.GetBindComponent<Image>(0);
			m_TxtM_Title = autoBindTool.GetBindComponent<TextMeshProUGUI>(1);
		}
	}
}
