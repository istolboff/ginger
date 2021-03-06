using System;

namespace Prolog.Engine.Miscellaneous
{
    using static Either;

    internal sealed record Either<TLeft, TRight>(TLeft? Left, TRight? Right, bool IsLeft)
    {
        public TResult Fold<TResult>(Func<TLeft, TResult> getFromLeft, Func<TRight, TResult> getFromRight) =>
            IsLeft ? getFromLeft(Left!) : getFromRight(Right!);

        public Either<TLeftResult, TRightResult> Fold2<TLeftResult, TRightResult>(
            Func<TLeft, TLeftResult> getFromLeft,
            Func<TRight, TRightResult> getFromRight) 
        =>
            IsLeft 
                ? Left<TLeftResult, TRightResult>(getFromLeft(Left!)) 
                : Right<TLeftResult, TRightResult>(getFromRight(Right!));

        public Either<TLeft, TResult> Map<TResult>(Func<TRight, TResult> getFromRight) =>
            IsLeft ? Left<TLeft, TResult>(Left!) : Right<TLeft, TResult>(getFromRight(Right!));

        public Either<TLeft, (TRight First, TRight1 Second)> Combine<TRight1>(Either<TLeft, TRight1> other) =>
            (IsLeft, other.IsLeft) switch
            {
                (true, _) => Left(Left!),
                (_, true) => Left(other.Left!),
                _ => Right((Right!, other.Right!))
            };

#pragma warning disable CA2225 // Provide a method named 'ToEither' as an alternate for operator op_Implicit
        public static implicit operator Either<TLeft, TRight>(syntacticshugar_EitherFromLeft<TLeft> eitherFromLeft) =>
            Left<TLeft, TRight>(eitherFromLeft.Left);

        public static implicit operator Either<TLeft, TRight>(syntacticshugar_EitherFromRight<TRight> eitherFromRight) =>
            Right<TLeft, TRight>(eitherFromRight.Right);
#pragma warning restore CA2225
    }

#pragma warning disable CA1707 // Remove the underscores from type name
// ReSharper disable InconsistentNaming
    internal sealed record syntacticshugar_EitherFromLeft<TLeft>(TLeft Left);

    internal sealed record syntacticshugar_EitherFromRight<TRight>(TRight Right);
// ReSharper restore InconsistentNaming
#pragma warning restore CA1707

    internal static class Either
    {
        public static Either<TLeft, TRight> Left<TLeft, TRight>(TLeft left) => 
            new (left, default, IsLeft: true);

        public static Either<TLeft, TRight> Right<TLeft, TRight>(TRight right) =>
            new (default, right, IsLeft: false);

        public static syntacticshugar_EitherFromLeft<T> Left<T>(T v) => 
            new (v);

        public static syntacticshugar_EitherFromRight<T> Right<T>(T v) => 
            new (v);

        public static Either<TLeft, TRight> Flatten<TLeft, TRight>(this Either<TLeft, Either<TLeft, TRight>> @this) =>
            @this.Fold(
                Left<TLeft, TRight>,
                right => right.Fold(Left<TLeft, TRight>, Right<TLeft, TRight>));
    }
}