namespace Icod.Expressions.Pcre;

internal sealed partial class PcreByteParser {
	private readonly record struct ByteClassValue( PcreByteSet Normal, PcreByteSet Logical, bool PositiveBitmap, bool NegativeBitmap );

	// Native ECLASS hoists ordinary byte bitmaps. Property-only XCLASS leaves
	// omit Any's bitmap until another operand/context requires one. Preserve both
	// projections until the root's positive bitmap requirement is known.
	private PcreByteSet ProjectByteExpression( PcreByteScalarExpression expression ) {
		var stack = new ByteClassValue[expression.StackDepth];
		var count = 0;
		foreach ( var term in expression.Terms.Span ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( term.Operand is { } operand ) { stack[count++] = ProjectByteOperand( operand ); continue; }
			if ( term.Operation == PcreScalarSetOperation.Not ) {
				var value = stack[count - 1];
				stack[count - 1] = new( value.Normal.Complement(), value.Logical.Complement(), value.NegativeBitmap, value.PositiveBitmap );
				continue;
			}
			var right = stack[--count];
			var left = stack[count - 1];
			var subtract = term.Operation == PcreScalarSetOperation.Subtract;
			stack[count - 1] = new( CombineByteClasses( left.Normal, right.Normal, term.Operation ),
				CombineByteClasses( left.Logical, right.Logical, term.Operation ),
				left.PositiveBitmap || ( subtract ? right.NegativeBitmap : right.PositiveBitmap ),
				left.NegativeBitmap || ( subtract ? right.PositiveBitmap : right.NegativeBitmap ) );
		}
		return stack[0].PositiveBitmap ? stack[0].Logical : stack[0].Normal;
	}

	private ByteClassValue ProjectByteOperand( PcreByteScalarSet operand ) {
		var bitmapSource = !operand.Ranges.IsEmpty;
		var actualProperty = false;
		var all = default(PcreByteSet);
		var withoutAny = default(PcreByteSet);
		foreach ( var range in operand.Ranges.Span ) {
			cancellationToken.ThrowIfCancellationRequested();
			var bytes = PcreByteSet.Range( (byte)range.Start, (byte)range.End );
			all = all.Union( operand.Caseless ? CloseByteCase( bytes ) : bytes );
		}
		foreach ( var predicate in operand.Predicates.Span ) {
			cancellationToken.ThrowIfCancellationRequested();
			var property = IsByteAlgebraProperty( predicate.Kind );
			actualProperty |= property;
			bitmapSource |= !property && predicate.Kind != PcreByteScalarClassKind.AnyScalar;
			var members = PcreByteSet.FromPredicate( value => {
				cancellationToken.ThrowIfCancellationRequested();
				return PcreScalarClassMembership.Contains( predicate.Kind, value, predicate.Category, operand.Caseless, predicate.PropertyValue ) != predicate.Negated;
			} );
			all = all.Union( members );
			if ( predicate.Kind != PcreByteScalarClassKind.AnyScalar ) { withoutAny = withoutAny.Union( members ); }
		}
		var normal = actualProperty && !bitmapSource ? withoutAny : all;
		var logical = all;
		if ( operand.Negated ) { normal = normal.Complement(); logical = logical.Complement(); }
		if ( actualProperty ) { return new( normal, logical, bitmapSource, bitmapSource ); }
		return new( normal, logical, NeedsBitmap( all, operand.Negated ), NeedsBitmap( all, !operand.Negated ) );
	}

	private static bool NeedsBitmap( PcreByteSet raw, bool negated ) {
		var members = negated ? raw.Complement() : raw;
		if ( members == PcreByteSet.AllBytes ) { return false; }
		return negated || members != default;
	}

	private static bool IsByteAlgebraProperty( PcreByteScalarClassKind kind ) => kind is not (
		PcreByteScalarClassKind.AnyScalar or PcreByteScalarClassKind.AsciiDigit or PcreByteScalarClassKind.AsciiWord
		or PcreByteScalarClassKind.AsciiSpace or PcreByteScalarClassKind.Ascii or PcreByteScalarClassKind.AsciiAlnum
		or PcreByteScalarClassKind.AsciiAlpha or PcreByteScalarClassKind.AsciiBlank or PcreByteScalarClassKind.AsciiControl
		or PcreByteScalarClassKind.AsciiGraph or PcreByteScalarClassKind.AsciiLower or PcreByteScalarClassKind.AsciiPrintable
		or PcreByteScalarClassKind.AsciiPunctuation or PcreByteScalarClassKind.AsciiUpper or PcreByteScalarClassKind.AsciiHexDigit
		or PcreByteScalarClassKind.HorizontalSpace or PcreByteScalarClassKind.VerticalSpace or PcreByteScalarClassKind.UnicodeBlank
	);

	private static PcreByteSet CombineByteClasses( PcreByteSet left, PcreByteSet right, PcreScalarSetOperation operation ) => operation switch {
		PcreScalarSetOperation.And => new( left.A & right.A, left.B & right.B, left.C & right.C, left.D & right.D ),
		PcreScalarSetOperation.Or => left.Union( right ),
		PcreScalarSetOperation.Subtract => new( left.A & ~right.A, left.B & ~right.B, left.C & ~right.C, left.D & ~right.D ),
		PcreScalarSetOperation.Xor => new( left.A ^ right.A, left.B ^ right.B, left.C ^ right.C, left.D ^ right.D ),
		_ => throw new InvalidOperationException( "Unknown binary class operator." )
	};
}
