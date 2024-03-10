namespace Source.Features.SceneEditor.Utils
{
    public static class ArrayUtils
    {
        public static T[] GetFlatArray<T>(T[,] array)
        {
            var result = new T[array.Length];
            var index = 0;
            
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    result[index++] = array[i, j];
                }
            }

            return result;
        }
    }
}