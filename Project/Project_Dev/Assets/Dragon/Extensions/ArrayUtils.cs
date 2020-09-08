using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ArrayUtils
{
    public static void CreateOrCopy<T>(ref T[] source,T[] from)
    {
        if (from == null || from.Length == 0)
        {
            source = Array.Empty<T>();
        }
        else
        {
            int finalLength = from.Length;
            CreateOrResize<T>(ref source, finalLength);
            Array.Copy(from, source, finalLength);
        }
    }

    public static void CreateOrResize<T>(ref T[] source, int finalLength)
    {
        if (finalLength == 0)
        {
            source = Array.Empty<T>();
        }
        else
        {
            if (source == null)
            {
                source = new T[finalLength];
            }else if ( source.Length != finalLength)
            {
                Array.Resize<T>(ref source, finalLength);
            }
        }
    }


    public static void Randomize(int[] arr, int n)
    {
        // Start from the last element and 
        // swap one by one. We don't need to run for the first element  
        // that's why i > 0 
        for (int i = n - 1; i > 0; i--)
        {
            // Pick a random index 
            // from 0 to i 
            int j = UnityEngine.Random.Range(0, i + 1);

            // Swap arr[i] with the  element at random index 
            int temp = arr[i];
            arr[i] = arr[j];
            arr[j] = temp;
        }
    }
}
