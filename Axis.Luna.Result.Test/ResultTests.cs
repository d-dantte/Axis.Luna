using System.Collections.Immutable;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace Axis.Luna.Result.Tests
{

    [TestClass]
    public class ResultTests
    {
        #region Of
        [TestMethod]
        public void Of_Tests()
        {
            var result = Result.Of(50m);
            Assert.IsInstanceOfType<DataResult<decimal>>(result);

            result = Result.Of<decimal>(new Exception());
            Assert.IsInstanceOfType<ErrorResult<decimal>>(result);

            Assert.ThrowsException<ArgumentNullException>(() => Result.Of(default(Func<decimal>)));

            result = Result.Of(() => 50m);
            Assert.IsInstanceOfType<DataResult<decimal>>(result);

            result = Result.Of(() => new Exception().Throw<decimal>());
            Assert.IsInstanceOfType<ErrorResult<decimal>>(result);

            Assert.ThrowsException<ArgumentNullException>(() => Result.Of(default(Func<IResult<decimal>>)));

            result = Result.Of(() => Result.Of(50m));
            Assert.IsInstanceOfType<DataResult<decimal>>(result);

            result = Result.Of(() => new Exception().Throw<IResult<decimal>>());
            Assert.IsInstanceOfType<ErrorResult<decimal>>(result);
        }
        #endregion

        #region Resolve
        [TestMethod]
        public void Resolve_Tests()
        {
            var result = default(IResult<int>);
            Assert.ThrowsException<ArgumentNullException>(() => result.Resolve());

            result = Result.Of(5);
            Assert.AreEqual(5, result.Resolve());

            result = Result.Of<int>(new AggregateException());
            Assert.ThrowsException<AggregateException>(() => result.Resolve());

            result = new UnknownResult<int>();
            Assert.ThrowsException<ArgumentException>(() => result.Resolve());
        }
        #endregion

        #region Is
        [TestMethod]
        public void Is_Tests()
        {
            var nullResult = default(IResult<string>);
            var dataResult = Result.Of("stuff");
            var errorResult = Result.Of<string>(new AccessViolationException("absolute violation"));
            var unknownResult = new UnknownResult<string>();

            // null
            Assert.ThrowsException<ArgumentNullException>(() => nullResult.IsDataResult());
            Assert.ThrowsException<ArgumentNullException>(() => nullResult.IsDataResult(out string _));
            Assert.ThrowsException<ArgumentNullException>(() => nullResult.IsErrorResult());
            Assert.ThrowsException<ArgumentNullException>(() => nullResult.IsErrorResult(out Exception _));
            Assert.ThrowsException<ArgumentNullException>(() => nullResult.IsErrorResult(out AccessViolationException _));

            // data
            Assert.IsTrue(dataResult.IsDataResult());
            Assert.IsTrue(dataResult.IsDataResult(out string data) && "stuff".Equals(data));
            Assert.IsFalse(dataResult.IsErrorResult());
            Assert.IsFalse(dataResult.IsErrorResult(out Exception _));
            Assert.IsFalse(dataResult.IsErrorResult(out AccessViolationException _));
            Assert.IsFalse(dataResult.IsErrorResult(out InvalidOperationException _));

            // error
            Assert.IsFalse(errorResult.IsDataResult());
            Assert.IsFalse(errorResult.IsDataResult(out data) && "stuff".Equals(data));
            Assert.IsTrue(errorResult.IsErrorResult());
            Assert.IsTrue(errorResult.IsErrorResult(out Exception _));
            Assert.IsTrue(errorResult.IsErrorResult(out AccessViolationException ex) && "absolute violation".Equals(ex.Message));
            Assert.IsFalse(errorResult.IsErrorResult(out InvalidOperationException _));

            // unkonwn
            Assert.IsFalse(unknownResult.IsDataResult());
            Assert.IsFalse(unknownResult.IsDataResult(out string _));
            Assert.IsFalse(unknownResult.IsErrorResult());
            Assert.IsFalse(unknownResult.IsErrorResult(out Exception _));
            Assert.IsFalse(unknownResult.IsErrorResult(out AccessViolationException _));
            Assert.IsFalse(unknownResult.IsErrorResult(out InvalidOperationException _));
        }
        #endregion

        #region With
        [TestMethod]
        public void With_Tests()
        {
            var nullResult = default(IResult<string>);
            var dataResult = Result.Of("stuff");
            var errorResult = Result.Of<string>(new Ex("eee"));
            var unknownResult = new UnknownResult<string>();

            // null
            Assert.ThrowsException<ArgumentNullException>(() => nullResult.WithData(Console.WriteLine));
            Assert.ThrowsException<ArgumentNullException>(() => nullResult.WithError(Console.WriteLine));
            Assert.ThrowsException<ArgumentNullException>(() => nullResult.WithError<string, Ex>(Console.WriteLine));

            // data
            string assignedData = null;
            var returnedResult = dataResult.WithData(d => assignedData = d);
            Assert.AreEqual("stuff", assignedData);
            Assert.AreEqual(dataResult, returnedResult);
            Assert.ThrowsException<ArgumentNullException>(() => dataResult.WithData(null));

            Exception assignedError = null;
            returnedResult = dataResult.WithError(e => assignedError = e);
            Assert.IsNull(assignedError);
            Assert.AreEqual(dataResult, returnedResult);
            Assert.ThrowsException<ArgumentNullException>(() => dataResult.WithError(null));

            assignedError = null;
            returnedResult = dataResult.WithError<string, Ex>(e => assignedError = e);
            Assert.IsNull(assignedError);
            Assert.AreEqual(dataResult, returnedResult);
            Assert.ThrowsException<ArgumentNullException>(() => dataResult.WithError<string, Ex>(null));

            assignedError = null;
            returnedResult = dataResult.WithError<string, ArgumentException>(e => assignedError = e);
            Assert.IsNull(assignedError);
            Assert.AreEqual(dataResult, returnedResult);

            // error
            assignedData = null;
            returnedResult = errorResult.WithData(d => assignedData = d);
            Assert.IsNull(assignedData);
            Assert.AreEqual(errorResult, returnedResult);
            Assert.ThrowsException<ArgumentNullException>(() => errorResult.WithData(null));

            assignedError = null;
            returnedResult = errorResult.WithError(e => assignedError = e);
            Assert.AreEqual("eee", assignedError.Message);
            Assert.IsInstanceOfType<Ex>(assignedError);
            Assert.AreEqual(errorResult, returnedResult);
            Assert.ThrowsException<ArgumentNullException>(() => errorResult.WithError(null));

            assignedError = null;
            returnedResult = errorResult.WithError<string, Ex>(e => assignedError = e);
            Assert.AreEqual("eee", assignedError.Message);
            Assert.IsInstanceOfType<Ex>(assignedError);
            Assert.AreEqual(errorResult, returnedResult);
            Assert.ThrowsException<ArgumentNullException>(() => dataResult.WithError<string, Ex>(null));

            assignedError = null;
            returnedResult = errorResult.WithError<string, ArgumentException>(e => assignedError = e);
            Assert.IsNull(assignedError);
            Assert.AreEqual(errorResult, returnedResult);

            // unknown
            Assert.ThrowsException<ArgumentException>(() => unknownResult.WithData(d => assignedData = d));
            Assert.ThrowsException<ArgumentNullException>(() => unknownResult.WithData(null));

            Assert.ThrowsException<ArgumentException>(() => unknownResult.WithError(e => assignedError = e));
            Assert.ThrowsException<ArgumentNullException>(() => unknownResult.WithError(null));

            Assert.ThrowsException<ArgumentException>(() => unknownResult.WithError<string, Ex>(e => assignedError = e));
            Assert.ThrowsException<ArgumentNullException>(() => unknownResult.WithError<string, Ex>(null));

            Assert.ThrowsException<ArgumentException>(() => unknownResult.WithError<string, ArgumentException>(e => assignedError = e));
        }
        #endregion

        #region MapError
        [TestMethod]
        public void MapError_Tests()
        {
            var nullResult = default(IResult<string>);
            var dataResult = Result.Of("stuff");
            var errorResult = Result.Of<string>(new Ex("eee"));
            var unknownResult = new UnknownResult<string>();

            // null
            Assert.ThrowsException<ArgumentNullException>(() => nullResult.MapError(e => e.ToString()));
            Assert.ThrowsException<ArgumentNullException>(() => nullResult.MapError((Ex e) => e.ToString()));

            // data
            string mappedResult = null;
            var returnedResult = dataResult.MapError(e => mappedResult = e.Message);
            Assert.AreEqual(dataResult, returnedResult);
            Assert.IsNull(mappedResult);
            Assert.ThrowsException<ArgumentNullException>(() => dataResult.MapError(null));

            mappedResult = null;
            returnedResult = dataResult.MapError((Ex e) => mappedResult = e.Message);
            Assert.AreEqual(dataResult, returnedResult);
            Assert.IsNull(mappedResult);
            Assert.ThrowsException<ArgumentNullException>(() => dataResult.MapError<string, Ex>(null));

            mappedResult = null;
            returnedResult = dataResult.MapError((Fx e) => mappedResult = e.Message);
            Assert.AreEqual(dataResult, returnedResult);
            Assert.IsNull(mappedResult);


            // error
            mappedResult = null;
            returnedResult = errorResult.MapError(e => mappedResult = e.Message);
            Assert.AreNotEqual(errorResult, returnedResult);
            Assert.AreEqual("eee", mappedResult);
            Assert.ThrowsException<ArgumentNullException>(() => errorResult.MapError(null));

            mappedResult = null;
            returnedResult = errorResult.MapError((Ex e) => mappedResult = e.Message);
            Assert.AreNotEqual(errorResult, returnedResult);
            Assert.AreEqual("eee", mappedResult);
            Assert.ThrowsException<ArgumentNullException>(() => errorResult.MapError<string, Ex>(null));

            mappedResult = null;
            returnedResult = errorResult.MapError((Fx e) => mappedResult = e.Message);
            Assert.AreEqual(errorResult, returnedResult);
            Assert.IsNull(mappedResult);


            // error
            Assert.ThrowsException<ArgumentNullException>(() => unknownResult.MapError(null));
            Assert.ThrowsException<ArgumentException>(() => unknownResult.MapError(e => e.Message));

            Assert.ThrowsException<ArgumentNullException>(() => unknownResult.MapError<string, Ex>(null));
            Assert.ThrowsException<ArgumentException>(() => unknownResult.MapError<string, Ex>(e => e.Message));
        }
        #endregion

        #region BindError
        [TestMethod]
        public void BindError_Tests()
        {
            var nullResult = default(IResult<string>);
            var dataResult = Result.Of("stuff");
            var errorResult = Result.Of<string>(new Ex("eee"));
            var unknownResult = new UnknownResult<string>();

            // null
            Assert.ThrowsException<ArgumentNullException>(() => nullResult.BindError(e => Result.Of(e.Message)));
            Assert.ThrowsException<ArgumentNullException>(() => nullResult.BindError((Ex e) => Result.Of(e.Message)));

            // data
            string mappedResult = null;
            var returnedResult = dataResult.BindError(e => Result.Of(mappedResult = e.Message));
            Assert.AreEqual(dataResult, returnedResult);
            Assert.IsNull(mappedResult);
            Assert.ThrowsException<ArgumentNullException>(() => dataResult.BindError(null));

            mappedResult = null;
            returnedResult = dataResult.BindError((Ex e) => Result.Of(mappedResult = e.Message));
            Assert.AreEqual(dataResult, returnedResult);
            Assert.IsNull(mappedResult);
            Assert.ThrowsException<ArgumentNullException>(() => dataResult.BindError<string, Ex>(null));

            mappedResult = null;
            returnedResult = dataResult.BindError((Fx e) => Result.Of(mappedResult = e.Message));
            Assert.AreEqual(dataResult, returnedResult);
            Assert.IsNull(mappedResult);


            // error
            mappedResult = null;
            returnedResult = errorResult.BindError(e => Result.Of(mappedResult = e.Message));
            Assert.AreNotEqual(errorResult, returnedResult);
            Assert.AreEqual("eee", mappedResult);
            Assert.ThrowsException<ArgumentNullException>(() => errorResult.BindError(null));

            mappedResult = null;
            returnedResult = errorResult.BindError((Ex e) => Result.Of(mappedResult = e.Message));
            Assert.AreNotEqual(errorResult, returnedResult);
            Assert.AreEqual("eee", mappedResult);
            Assert.ThrowsException<ArgumentNullException>(() => errorResult.BindError<string, Ex>(null));

            mappedResult = null;
            returnedResult = errorResult.BindError((Fx e) => Result.Of(mappedResult = e.Message));
            Assert.AreEqual(errorResult, returnedResult);
            Assert.IsNull(mappedResult);


            // error
            Assert.ThrowsException<ArgumentNullException>(() => unknownResult.BindError(null));
            Assert.ThrowsException<ArgumentException>(() => unknownResult.BindError(e => Result.Of(e.Message)));

            Assert.ThrowsException<ArgumentNullException>(() => unknownResult.BindError<string, Ex>(null));
            Assert.ThrowsException<ArgumentException>(() => unknownResult.BindError<string, Ex>(e => Result.Of(e.Message)));
        }
        #endregion

        #region ConsumeError
        [TestMethod]
        public void ConsumeError_Tests()
        {
            var nullResult = default(IResult<string>);
            var dataResult = Result.Of("stuff");
            var errorResult = Result.Of<string>(new Ex("eee"));
            var unknownResult = new UnknownResult<string>();

            // null
            Assert.ThrowsException<ArgumentNullException>(() => nullResult.ConsumeError(e => e.ToString()));
            Assert.ThrowsException<ArgumentNullException>(() => nullResult.ConsumeError((Ex e) => e.ToString()));

            // data
            string mappedResult = null;
            dataResult.ConsumeError(e => mappedResult = e.Message);
            Assert.IsNull(mappedResult);
            Assert.ThrowsException<ArgumentNullException>(() => dataResult.ConsumeError(null));

            mappedResult = null;
            dataResult.ConsumeError((Ex e) => mappedResult = e.Message);
            Assert.IsNull(mappedResult);
            Assert.ThrowsException<ArgumentNullException>(() => dataResult.ConsumeError<string, Ex>(null));

            mappedResult = null;
            dataResult.ConsumeError((Fx e) => mappedResult = e.Message);
            Assert.IsNull(mappedResult);


            // error
            mappedResult = null;
            errorResult.ConsumeError(e => mappedResult = e.Message);
            Assert.AreEqual("eee", mappedResult);
            Assert.ThrowsException<ArgumentNullException>(() => errorResult.ConsumeError(null));

            mappedResult = null;
            errorResult.ConsumeError((Ex e) => mappedResult = e.Message);
            Assert.AreEqual("eee", mappedResult);
            Assert.ThrowsException<ArgumentNullException>(() => errorResult.ConsumeError<string, Ex>(null));

            mappedResult = null;
            errorResult.ConsumeError((Fx e) => mappedResult = e.Message);
            Assert.IsNull(mappedResult);


            // error
            Assert.ThrowsException<ArgumentNullException>(() => unknownResult.ConsumeError(null));
            Assert.ThrowsException<ArgumentException>(() => unknownResult.ConsumeError(Console.Write));

            Assert.ThrowsException<ArgumentNullException>(() => unknownResult.ConsumeError<string, Ex>(null));
            Assert.ThrowsException<ArgumentException>(() => unknownResult.ConsumeError<string, Ex>(Console.Write));
        }
        #endregion

        #region TransformError
        [TestMethod]
        public void TransformError_Tests()
        {
            var nullResult = default(IResult<string>);
            var dataResult = Result.Of("stuff");
            var errorResult = Result.Of<string>(new Ex("eee"));
            var unknownResult = new UnknownResult<string>();

            // null
            Assert.ThrowsException<ArgumentNullException>(() => nullResult.TransformError(e => new Fx(e.Message)));
            Assert.ThrowsException<ArgumentNullException>(() => nullResult.TransformError((Ex e) => new Fx(e.Message)));

            // data
            var returnedResult = dataResult.TransformError(e => new Fx(e.Message));
            Assert.AreEqual(dataResult, returnedResult);
            Assert.ThrowsException<ArgumentNullException>(() => dataResult.TransformError(null));

            returnedResult = dataResult.TransformError((Ex e) => new Fx(e.Message));
            Assert.AreEqual(dataResult, returnedResult);
            Assert.ThrowsException<ArgumentNullException>(() => dataResult.TransformError<string, Ex>(null));

            returnedResult = dataResult.TransformError((Fx e) => new Fx(e.Message));
            Assert.AreEqual(dataResult, returnedResult);


            // error
            returnedResult = errorResult.TransformError(e => new Fx(e.Message));
            Assert.AreNotEqual(errorResult, returnedResult);
            Assert.IsInstanceOfType<ErrorResult<string>>(returnedResult);
            Assert.ThrowsException<ArgumentNullException>(() => errorResult.TransformError(null));

            returnedResult = errorResult.TransformError((Ex e) => new Fx(e.Message));
            Assert.AreNotEqual(errorResult, returnedResult);
            Assert.IsInstanceOfType<ErrorResult<string>>(returnedResult);
            Assert.ThrowsException<ArgumentNullException>(() => errorResult.TransformError<string, Ex>(null));

            returnedResult = errorResult.TransformError((Fx e) => new Fx(e.Message));
            Assert.AreEqual(errorResult, returnedResult);


            // error
            Assert.ThrowsException<ArgumentNullException>(() => unknownResult.TransformError(null));
            Assert.ThrowsException<ArgumentException>(() => unknownResult.TransformError(e => new Fx(e.Message)));

            Assert.ThrowsException<ArgumentNullException>(() => unknownResult.TransformError<string, Ex>(null));
            Assert.ThrowsException<ArgumentException>(() => unknownResult.TransformError<string, Ex>(e => new Fx(e.Message)));
        }
        #endregion

        #region Continue
        [TestMethod]
        public void Continue_Tests()
        {
            var nullResult = default(IResult<string>);
            var dataResult = Result.Of("stuff");
            var errorResult = Result.Of<string>(new Ex("eee"));
            var unknownResult = new UnknownResult<string>();

            // null
            Assert.ThrowsException<ArgumentNullException>(() => nullResult.Continue(e => e.ToString().Length));

            // data
            var returnedResult = dataResult.Continue(e => e.ToString().Length);
            Assert.AreEqual(5, returnedResult.Resolve());
            Assert.ThrowsException<ArgumentNullException>(() => dataResult.Continue<string, int>(null));


            // error
            returnedResult = errorResult.Continue(e => (e as Ex).Message.Length);
            Assert.AreEqual(3, returnedResult.Resolve());
            Assert.ThrowsException<ArgumentNullException>(() => errorResult.Continue<string, int>(null));


            // unknown
            Assert.ThrowsException<ArgumentNullException>(() => unknownResult.Continue<string, int>(null));
            Assert.ThrowsException<ArgumentException>(() => unknownResult.Continue(e => e.ToString().Length));
        }
        #endregion

        #region Fold
        #endregion

        #region Nested Test Types
        internal class UnknownResult<T> : IResult<T>
        {
            public IResult<TOut> Bind<TOut>(Func<T, IResult<TOut>> binder)
            {
                throw new NotImplementedException();
            }

            public void Consume(Action<T> consumer)
            {
                throw new NotImplementedException();
            }

            public IResult<TOut> Map<TOut>(Func<T, TOut> mapper)
            {
                throw new NotImplementedException();
            }

            public IResult<TOut> MapAs<TOut>()
            {
                throw new NotImplementedException();
            }
        }

        internal class Ex : Exception
        {
            public Ex(string message)
            : base(message) { }
        }

        internal class Fx : Exception
        {
            public Fx(string message)
            : base(message) { }
        }
        #endregion
    }

    public readonly struct Suid
    {
        public static Suid Create() => new();

        public static Suid Parse(string text)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Represents an indexed chunk of continguous data from a stream of data
    /// </summary>
    /// <typeparam name="TData">The type of data</typeparam>
    public readonly struct Page<TData>
    {
        public int Count { get; }
        public int Offset { get; }
        public int SequenceLength { get; }
        public int MaxPageLength { get; }
        public int PageIndex { get; }

        public ImmutableArray<TData> Data{ get; }
    }

    public interface IRepository<TId, TEntity>
    {
        Task<TEntity> GetEntity(TId id);

        Task<TEntity> CreateEntity(TEntity entity);

        Task<TEntity> UpdateEntity(TEntity entity);

        Task DeleteEntity(TEntity entity);
    }

    #region Async Operations Audit Service (for commands and streams)

    public enum AsyncOperationType
    {
        Command,
        Stream
    }

    public readonly struct AsyncOperationIdentifier
    {
        public const string NID = "asyncop";

        public static readonly Regex Pattern = new(
            @"^urn:asyncop:(?<operation>cmd|seq):(?<domain>\*|([a-zA-Z_]+[a-zA-Z_\.-]*)):(?<guid>[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})$",
            RegexOptions.Compiled);

        public string Domain { get; }

        public Suid UUId { get; }

        public AsyncOperationType OperationType { get; }

        public AsyncOperationIdentifier(AsyncOperationType type, string domain, Suid uuid)
        {
            var nullDomain = "*".Equals(domain);
            Domain = nullDomain ? null! : domain;
            UUId = uuid;
            OperationType = type; // throw if not defined

            if ((nullDomain && !Guid.Empty.Equals(UUId))
                || !Pattern.IsMatch(ToString()))
                throw new ArgumentOutOfRangeException(
                    nameof(domain), $"Invalid operation-identifier format: {ToString()}");
        }

        public AsyncOperationIdentifier(
            AsyncOperationType type,
            string domain)
            : this(type, domain, Suid.Create()) // default to the sequential GUid
        {
        }

        public static AsyncOperationIdentifier Of(AsyncOperationType type,  string domain, Suid uuid) => new(type, domain, uuid);

        public static AsyncOperationIdentifier Of(string id) => Parse(id);

        public static implicit operator AsyncOperationIdentifier((AsyncOperationType type, string domain, Suid id) info) => new(info.type, info.domain, info.id);

        public static implicit operator AsyncOperationIdentifier(string id) => Parse(id);

        #region Parse
        public static AsyncOperationIdentifier Parse(string identifier)
        {
            _ = !TryParse(identifier, out var result);
            return result.Resolve();
        }

        public static bool TryParse(string identifierText, out IResult<AsyncOperationIdentifier> identifierResult)
        {
            var match = Pattern.Match(identifierText);

            identifierResult = match.Success switch
            {
                false => Result.Of<AsyncOperationIdentifier>(new FormatException($"Invalid command-identifier format: [{identifierText}]")),
                true => Result.Of(
                    new AsyncOperationIdentifier(
                        ParseOperationType(match.Groups["operation"].Value),
                        match.Groups["domain"].Value,
                        Suid.Parse(match.Groups["guid"].Value)))
            };

            return identifierResult.IsDataResult();
        }

        private static string ToText(AsyncOperationType type)
        {
            return type switch
            {
                AsyncOperationType.Command => "cmd",
                AsyncOperationType.Stream => "seq",
                _ => throw new ArgumentException($"Invalid type: {type}")
            };
        }

        private static AsyncOperationType ParseOperationType(string type)
        {
            return type switch
            {
                "cmd" or "Command" => AsyncOperationType.Command,
                "seq" or "Stream" => AsyncOperationType.Stream,
                _ => throw new ArgumentException($"Invalid type: {type}")
            };
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return $"urn:{NID}:{ToText(OperationType)}:{Domain ?? "*"}:{UUId}";
        }
        #endregion
    }

    public enum CommandStatus
    {
        Pending = 0,
        Faulted,
        Completed,
        Unknown
    }

    public enum StreamStatus
    {
        Live = 0,
        Faulted,
        Completed,
        Unknown
    }

    public record AsyncOperationAuditRecord<TOperationId, TStatus>
        where TStatus : struct, Enum
    {
        public required AsyncOperationIdentifier Identifier { get; init; }

        public required TOperationId OperationId { get; init; }

        public required TStatus Status { get; set; }

        public required DateTimeOffset CreatedOn { get; init; }

        public DateTimeOffset? ModifiedOn { get; set; }
    }

    public interface IAsyncOperationsAuditor<TId, TStatus>
        where TStatus : struct, Enum
    {
        Task<AsyncOperationIdentifier> CreateAuditRecord(TId streamId, string domainNamespace);
        Task UpdateStatus(AsyncOperationIdentifier auditRecordIdentifier, TStatus newStatus);
        Task<TStatus> GetStatus(AsyncOperationIdentifier auditRecordIdentifier);
    }

    public class AsyncStreamAuditor<TStreamId> : IAsyncOperationsAuditor<TStreamId, StreamStatus>
    {
        private readonly IRepository<AsyncOperationIdentifier, AsyncOperationAuditRecord<TStreamId, StreamStatus>> _repository;

        public AsyncStreamAuditor(IRepository<AsyncOperationIdentifier, AsyncOperationAuditRecord<TStreamId, StreamStatus>> repository)
        {
            ArgumentNullException.ThrowIfNull(repository);

            _repository = repository;
        }

        public async Task<AsyncOperationIdentifier> CreateAuditRecord(TStreamId streamId, string domainNamespace)
        {
            var record = new AsyncOperationAuditRecord<TStreamId, StreamStatus>
            {
                CreatedOn = DateTimeOffset.Now,
                OperationId = streamId,
                Status = StreamStatus.Live,
                Identifier = new AsyncOperationIdentifier(AsyncOperationType.Stream, domainNamespace),
            };

            record = await _repository.CreateEntity(record);
            return record.Identifier;
        }

        public async Task<StreamStatus> GetStatus(AsyncOperationIdentifier auditRecordIdentifier)
        {
            var record = await _repository.GetEntity(auditRecordIdentifier);
            return record.Status;
        }

        public async Task UpdateStatus(AsyncOperationIdentifier auditRecordIdentifier, StreamStatus newStatus)
        {
            if (!Enum.IsDefined<StreamStatus>(newStatus))
                throw new ArgumentOutOfRangeException(nameof(newStatus), $"Invalid status: {newStatus}");

            var record = await _repository.GetEntity(auditRecordIdentifier);
            record.Status = newStatus;
            record.ModifiedOn = DateTimeOffset.Now;

            _ = await _repository.UpdateEntity(record);
        }
    }

    public class AsyncCommandAuditor<TCommandId> : IAsyncOperationsAuditor<TCommandId, CommandStatus>
    {
        private readonly IRepository<AsyncOperationIdentifier, AsyncOperationAuditRecord<TCommandId, CommandStatus>> _repository;

        public AsyncCommandAuditor(IRepository<AsyncOperationIdentifier, AsyncOperationAuditRecord<TCommandId, CommandStatus>> repository)
        {
            ArgumentNullException.ThrowIfNull(repository);

            _repository = repository;
        }

        public async Task<AsyncOperationIdentifier> CreateAuditRecord(TCommandId streamId, string domainNamespace)
        {
            var record = new AsyncOperationAuditRecord<TCommandId, CommandStatus>
            {
                CreatedOn = DateTimeOffset.Now,
                OperationId = streamId,
                Status = CommandStatus.Pending,
                Identifier = new AsyncOperationIdentifier(AsyncOperationType.Command, domainNamespace),
            };

            record = await _repository.CreateEntity(record);
            return record.Identifier;
        }

        public async Task<CommandStatus> GetStatus(AsyncOperationIdentifier auditRecordIdentifier)
        {
            var record = await _repository.GetEntity(auditRecordIdentifier);
            return record.Status;
        }

        public async Task UpdateStatus(AsyncOperationIdentifier auditRecordIdentifier, CommandStatus newStatus)
        {
            if (!Enum.IsDefined<CommandStatus>(newStatus))
                throw new ArgumentOutOfRangeException(nameof(newStatus), $"Invalid status: {newStatus}");

            var record = await _repository.GetEntity(auditRecordIdentifier);
            record.Status = newStatus;
            record.ModifiedOn = DateTimeOffset.Now;

            _ = await _repository.UpdateEntity(record);
        }
    }
    #endregion

    #region Identity

    public enum MonikerType
    {
        Unknown,
        User,
        System
    }

    public readonly struct Moniker
    {
        internal static readonly Regex Pattern = new(
            @"^(\@|\$)[a-zA-Z0-9]([\.-]?[a-zA-Z0-9])*$",
            RegexOptions.Compiled);

        private readonly string _name;

        public bool IsDefault => _name is null;

        public static Moniker Default => default;

        public MonikerType Type => _name switch
        {
            null => MonikerType.Unknown,
            string moniker => moniker[0] switch
            {
                '@' => MonikerType.User,
                '$' => MonikerType.System,
                _ => MonikerType.Unknown
            }
        };

        public Moniker(string moniker)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(moniker);

            if (!Pattern.IsMatch(moniker))
                throw new FormatException($"Invalid moniker format: {moniker}");

            _name = moniker;
        }

        public override string ToString() => IsDefault ? "*" : _name;

        public static Moniker Of(string moniker) => new(moniker);

        public static implicit operator Moniker(string moniker) => new(moniker);
    }

    public enum PrincipalStatus
    {
        Active,
        Deleted
    }

    public record Principal
    {
        public required Suid Id { get; init; }

        public required Moniker Moniker { get; init; }

        public required PrincipalStatus Status { get; init; }

        public required DateTimeOffset CreatedOn { get; init; }

        public DateTimeOffset? ModifiedOn { get; set; }
    }

    #endregion
}
