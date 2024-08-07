using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Connection : IEquatable<Connection>
{
    public int pieceId;
    public int orientation;

    #region Equality
    public override readonly bool Equals(object obj)
    {
        if (obj is Connection connection && connection.pieceId == pieceId && connection.orientation == orientation) return true;
        else return base.Equals(obj);
    }

    public readonly bool Equals(Connection other)
    {
        return pieceId == other.pieceId &&
               orientation == other.orientation;
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(pieceId, orientation);
    }

    public static bool operator ==(Connection left, Connection right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Connection left, Connection right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return $"Piece: {pieceId} Orientation: {orientation}";
    }
    #endregion
}
