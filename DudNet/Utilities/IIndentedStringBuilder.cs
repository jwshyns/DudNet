namespace DudNet.Utilities;

internal interface IIndentedStringBuilder
{
	IIndentedStringBuilder Append(char character);
	IIndentedStringBuilder AppendLine();
	IIndentedStringBuilder AppendLine(char character);
	IIndentedStringBuilder AppendLine(string line);
	IIndentedStringBuilder BlockWrite(Action<IIndentedStringBuilder> action);
	IIndentedStringBuilder IndentedBlockWrite(Action<IIndentedStringBuilder> action);
}