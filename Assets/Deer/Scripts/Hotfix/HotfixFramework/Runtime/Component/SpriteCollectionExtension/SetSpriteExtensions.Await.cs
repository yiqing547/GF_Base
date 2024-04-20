using UnityEngine.UI;

namespace UGFExtensions.SpriteCollection
{
    public static partial class SetSpriteExtensions
    {
        /// <summary>
        /// 设置精灵
        /// </summary>
        /// <param name="image"></param>
        /// <param name="collectionPath">精灵所在收集器地址</param>
        /// <param name="spritePath">精灵名称</param>
        public static void SetSpriteAsync(this Image image, string collectionPath, string spritePath)
        {
            GameEntry.SpriteCollection.SetSpriteAsync(WaitSetImage.Create(image, collectionPath, spritePath));
        }

        /// <summary>
        /// 设置精灵，collectionName图集名，spriteName图片名
        /// </summary>
        /// <param name="image"></param>
        /// <param name="groupName"></param>
        /// <param name="collectionName"></param>
        /// <param name="spriteName"></param>
        public static void SetSpriteByNameAsync(this Image image, string collectionName, string spriteName)
        {
            string collectionPath = AssetUtility.UI.GetSpriteCollectionPath(collectionName);
            string spritePath = AssetUtility.UI.GetSpritePath($"{collectionName}/{spriteName}");
            GameEntry.SpriteCollection.SetSpriteAsync(WaitSetImage.Create(image, collectionPath, spritePath));
        }
    }
}