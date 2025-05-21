using Newtonsoft.Json;
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

    public readonly struct Optional<TValue> where TValue : class
    {
        private readonly TValue? _value;
    }

    // add another integer (4bytes) to the random fields, bringing it to a total of 22 bytes
    // 8 (timestamp), 2 (prefix), 8 (random), 4 (random)
    public readonly struct Suid
    {
        public static Suid Create() => new();

        public static Suid Parse(string text)
        {
            throw new NotImplementedException();
        }
    }

    public interface IEntity<TId>
    {
        TId Id { get; }

        DateTimeOffset CreatedOn { get; init; }

        DateTimeOffset? ModifiedOn { get; set; }
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

    public readonly struct PageRequest { }

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

    #region Wallet
    public enum WalletStatus
    {
        Active,
        Disabled
    }

    public enum WalletType
    {
        Funded,
        Replay
    }

    public enum TransactionStatus
    {
        /// <summary>
        /// Transaction has been lodged. Value exchange is still pending
        /// <para/>
        /// <c>*-> Pending -> Declined ||</c>
        /// </summary>
        Pending,

        /// <summary>
        /// Value was not transferred. Transaction is declined.
        /// <para/>
        /// <c>*-> Pending -> Declined ||</c>
        /// </summary>
        Declined,

        /// <summary>
        /// Value exchange is verified: credit/debit done.
        /// <para/>
        /// <c>*-> Pending -> Committed |</c>
        /// </summary>
        Committed,

        /// <summary>
        /// Value was reset, and verified. This can only be done if the transaction was previously <see cref="Committed"/>
        /// <para/>
        /// <c>*-> Pending -> Committed |-> Reversed</c>
        /// </summary>
        Reversed
    }

    public enum TransactionType
    {
        /// <summary>
        /// In the context of the wallet, this is a deduction-type transaction
        /// </summary>
        Debit,

        /// <summary>
        /// In the context of the wallet, this is an addition-type transaction
        /// </summary>
        Credit
    }


    /// <summary>
    /// There are two wallets: FundedWallet, and ReplayWallet
    /// </summary>
    public abstract record UserWallet<TWalletTransaction, TOrderItem> :
        IEntity<Suid>
        where TOrderItem : IOrderItem
        where TWalletTransaction : WalletTransaction<TOrderItem>
    {
        #region Entity
        public required Suid Id { get; init; }

        public required DateTimeOffset CreatedOn { get; init; }

        public DateTimeOffset? ModifiedOn { get; set; }
        #endregion

        public required Suid UserId { get; init; }

        /// <summary>
        /// Can this be made into an integer type?
        /// </summary>
        public decimal Balance { get; init; }

        public WalletStatus Status { get; set; }

        public abstract WalletType Type { get; }

        /// <summary>
        /// Joined from the WalletTransaction collection when needed
        /// </summary>
        public abstract ImmutableArray<TWalletTransaction> RecentTransactions { get; init; }
    }

    public record FundedWallet : UserWallet<WalletTransaction<IOrderItem>, IOrderItem>
    {
        public override WalletType Type => WalletType.Funded;

        public override ImmutableArray<WalletTransaction<IOrderItem>> RecentTransactions { get; init; } = [];
    }

    public record ReplayWallet: UserWallet<WalletTransaction<IReplayItem>, IReplayItem>
    {
        public override WalletType Type => WalletType.Replay;

        public override ImmutableArray<WalletTransaction<IReplayItem>> RecentTransactions { get; init; } = [];
    }

    /// <summary>
    /// Base interface for all OrderItems - elements that can be paid for, that may or may not be tied to a product/sku
    /// </summary>
    public interface IOrderItem : IEntity<Suid>
    {
    }

    /// <summary>
    /// Base interface for replay items
    /// </summary>
    public interface IReplayItem : IOrderItem
    {
    }

    public record WalletTransaction<TOrderItem> : IEntity<Suid>
    {
        public required Suid Id { get; init; }

        public required DateTimeOffset CreatedOn { get; init; }

        public required DateTimeOffset? ModifiedOn { get; set; }


        public required TransactionStatus Status { get; set; }

        public required TransactionType Type { get; init; }

        public required Suid WalletId { get; init; }

        /// <summary>
        /// A snapshot of the balance when the transaction was issued
        /// </summary>
        public required decimal BalanceSnapshot { get; init; }

        /// <summary>
        /// The amount for this transaction
        /// </summary>
        public required decimal TransactionAmount { get; init; }

        /// <summary>
        /// The item for which this transaction was issued
        /// </summary>
        public required IOrderItem OrderItem { get; init; }
    }


    #region Services
    public interface IWalletManager
    {
        Task<Optional<WalletTuple>> GetWallets(Suid userId);

        /// <summary>
        /// For now, this should be called only once per user. So a check must be made for already created wallets
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<WalletTuple> CreateWallets(Suid userId);

        Task<Page<FundedWallet>> GetFundedWallets(PageRequest request);

        Task<Page<ReplayWallet>> GetReplayWallets(PageRequest request);

        Task DisableWallet(Suid walletId);

        Task ActivateWallet(Suid wallet);

        #region Request/Response
        public record WalletTuple
        {
            public required FundedWallet FundedWallet { get; init; }

            public required ReplayWallet ReplayWallet { get; init; }
        }
        #endregion
    }

    public interface IWalletTransactionAuthority
    {
        Task<WalletTransaction<TItem>> LodgeTransaction<TItem>(TransactionLodgeInfo<TItem> request) where TItem : IOrderItem;

        Task UpdateTransactionStatus(TransactionStatusUpdate updateRequest);


        Task<Page<WalletTransaction<IOrderItem>>> GetFundedTransactions(TransactionListRequest request);

        Task<Page<WalletTransaction<IReplayItem>>> GetReplayTransactions(TransactionListRequest request);


        Task<Page<WalletTransaction<IOrderItem>>> GetAllFundedTransactions(PageRequest request);

        Task<Page<WalletTransaction<IReplayItem>>> GetAllReplayTransactions(PageRequest request);


        #region Request/Response
        public record TransactionLodgeInfo<TItem> where TItem : IOrderItem
        {
            public required TransactionType Type { get; init; }

            public required decimal Amount { get; init; }

            public required TItem OrderItem { get; init; }

            public required Suid WalletId { get; init; }
        }

        public record TransactionStatusUpdate
        {
            public required Suid TransactionId { get; init; }

            public required TransactionStatus NewStatus { get; init; }
        }

        public record TransactionListRequest
        {
            public required Suid UserId { get; init; }

            public required PageRequest PageRequest { get; init; }
        }
        #endregion
    }
    #endregion


    #region SNL Order Items
    public record SNLTile : IOrderItem
    {
        #region Entity
        public required Suid Id { get; init; }

        public required DateTimeOffset CreatedOn { get; init; }

        public DateTimeOffset? ModifiedOn { get; set; }
        #endregion

        /// <summary>
        /// The user that is making the purchase
        /// </summary>
        public required Suid UserId { get; init; }

        public required Suid GamePresetId { get; init; }

        public required Suid GridId { get; init; }

        public required ushort TileIndex { get; init; }
    }

    public record SNLReplayTile : SNLTile, IReplayItem
    {
    }
    #endregion

    #endregion

    #region KYC

    [Flags]
    public enum CustomerInfoAttributes
    {
        None = 0,
        Required = 1,
        Verifiable = 2
    }

    public enum InfoVerificationStatus
    {
        Ignored,
        Pending,
        Verified,
        Unverified,
        Flagged
    }

    public enum ContactType
    {
        Email,
        Phone
    }

    public enum AddressComponent
    {
        Door,
        Floor,
        Wing,
        BuildingName,
        House,

        Street, Road,

        District,
        Village,
        Zone,
        Sector,
        Municipality,

        PostCode, ZipCode,

        Town,
        City,
        County, Parish, Borough,
        Province,
        State,
        Region,
        FederalDistrict,

        Country
    }

    public interface ICustomerInfo
    {
        CustomerInfoAttributes Attributes { get; }

        InfoVerificationStatus VerificationStatus { get; set; }
    }

    public enum UserAccountStatus
    {
        Unverified,
        Verified,

        /// <summary>
        /// Account is suspended - user can perform only limited operations
        /// </summary>
        Suspended,

        /// <summary>
        /// Account is deactivated - user cannot perform any operations with the account
        /// </summary>
        Deactivated
    }

    public record UserAccount : IEntity<Suid>
    {
        #region Entity
        public required Suid Id { get; init; }

        public required DateTimeOffset CreatedOn { get; init; }

        public DateTimeOffset? ModifiedOn { get; set; }
        #endregion

        public required Moniker Moniker { get; init; }

        public required Suid UserId { get; init; }

        public required UserAccountStatus Status { get; init; }

        public required PrimaryNames Appelation { get; init; }

        public required List<FormerNames> PreviousAppelation { get; set; } = [];

        public required List<ContactInfo> Contact { get; init; } = [];

        public required AddressInfo Address { get; init; }

        public required List<Document> AdditionalDocuments { get; init; } = [];

        #region Nested types
        public record PrimaryNames : ICustomerInfo
        {
            #region CustomerInfo
            public required CustomerInfoAttributes Attributes { get; init; } = CustomerInfoAttributes.Required;

            public InfoVerificationStatus VerificationStatus { get; set; } = InfoVerificationStatus.Ignored;
            #endregion

            public string? Title { get; init; }

            public required string FirstName { get; init; }

            public required string MiddleNames { get; init; }

            public required string LastName { get; init; }
        }

        public record FormerNames : ICustomerInfo
        {
            #region CustomerInfo
            public required CustomerInfoAttributes Attributes { get; init; }

            public InfoVerificationStatus VerificationStatus { get; set; } = InfoVerificationStatus.Ignored;
            #endregion

            public string? FirstName { get; set; }

            public string? MiddleNames { get; set; }

            public string? LastName { get; set; }

            public bool TryValidate(out string[] errors)
            {
                if (string.IsNullOrWhiteSpace(FirstName)
                    && string.IsNullOrWhiteSpace(MiddleNames)
                    && string.IsNullOrWhiteSpace(LastName))
                    errors = ["Invalid state: must contain at least 1 former name"];

                else errors = [];

                return errors.Length == 0;
            }
        }

        public record ContactInfo : ICustomerInfo
        {
            #region CustomerInfo
            public required CustomerInfoAttributes Attributes { get; init; } = CustomerInfoAttributes.None;

            public InfoVerificationStatus VerificationStatus { get; set; } = InfoVerificationStatus.Ignored;
            #endregion

            public required ContactType Type { get; init; }

            public required string Data { get; init; }

        }

        public record AddressInfo : ICustomerInfo
        {
            #region CustomerInfo
            public required CustomerInfoAttributes Attributes { get; init; } = CustomerInfoAttributes.None;

            public InfoVerificationStatus VerificationStatus { get; set; } = InfoVerificationStatus.Ignored;
            #endregion

            private readonly Dictionary<AddressComponent, string> _components = [];

            public bool TryGetComponent(
                AddressComponent component,
                out string? info)
                => _components.TryGetValue(component, out info);

            public string GetComponent(AddressComponent component) => _components[component];

            public string GetOrAdd(AddressComponent component, Func<AddressComponent, string> componentFactory)
            {
                ArgumentNullException.ThrowIfNull(componentFactory);

                if (_components.TryGetValue(component, out var info))
                    return info;

                else return _components[component] = componentFactory.Invoke(component);
            }

            public ImmutableArray<(AddressComponent Component, string Info)> Components() => [.. _components.Select(kvp => (kvp.Key, kvp.Value))];
        }

        public record Document : ICustomerInfo
        {
            #region CustomerInfo
            public required CustomerInfoAttributes Attributes { get; init; } = CustomerInfoAttributes.None;

            public InfoVerificationStatus VerificationStatus { get; set; } = InfoVerificationStatus.Ignored;
            #endregion

            public required string DocumentName { get; init; }

            public required string Url { get; init; }

            public required ImmutableHashSet<MimeTypeName> SupportedTypes { get; init; }

            public required ushort MaxFileSizeKb { get; init; }
        }
        #endregion
    }

    public interface IUserAccountManager
    {

    }

    #region Mimes
    // --- MIME Type Names Enum ---
    public enum MimeTypeName
    {
        Unknown = 0, // Default for unhandled cases

        // Application Mime Types
        Ez,
        Aw,
        Atom,
        AtomCat,
        AtomSvc,
        Ccxml,
        Cdmia,
        Cdmic,
        Cdmid,
        Cdmio,
        Cdmiq,
        Cu,
        Davmount,
        Dbk,
        Dssc,
        Xdssc,
        Ecma,
        Emma,
        Epub,
        Exi,
        Pfr,
        Gml,
        Gpx,
        Gxf,
        Stk,
        Ink,
        Inkml,
        Ipfix,
        Jar,
        Ser,
        Class,
        Json,
        Jsonml,
        LostXml,
        Hqx,
        Cpt,
        Mads,
        Mrc,
        Mrcx,
        Ma,
        Nb,
        Mb,
        Mathml,
        Mbox,
        Mscml,
        Metalink,
        Meta4,
        Mets,
        Mods,
        M21,
        Mp21,
        Mp4s,
        Doc,
        Dot,
        Mxf,
        Bin,
        Dms,
        Lrf,
        Mar,
        So,
        Dist,
        Distz,
        Pkg,
        Bpk,
        Dump,
        Elc,
        Deploy,
        Oda,
        Opf,
        Ogx,
        Omdoc,
        Onetoc,
        Onetoc2,
        Onetmp,
        Onepkg,
        Oxps,
        Xer,
        Pdf,
        Pgp,
        Asc,
        Sig,
        Prf,
        P10,
        P7m,
        P7c,
        P7s,
        P8,
        Ac,
        Cer,
        Crl,
        Pkipath,
        Pki,
        Pls,
        Ai,
        Eps,
        Ps,
        Cww,
        Pskcxml,
        Rdf,
        Rif,
        Rnc,
        Rl,
        Rld,
        Rs,
        Gbr,
        Mft,
        Roa,
        Rsd,
        Rss,
        Rtf,
        Sbml,
        Scq,
        Scs,
        Spq,
        Spp,
        Sdp,
        Setpay,
        Setreg,
        Shf,
        Smi,
        Smil,
        Rq,
        Srx,
        Gram,
        Grxml,
        Sru,
        Ssdl,
        Ssml,
        Tei,
        Teicorpus,
        Tfi,
        Tsd,
        Plb,
        Psb,
        Pvb,
        Tcap,
        Pwn,
        Aso,
        Imp,
        Acu,
        Atc,
        Acutc,
        Air,
        Fcdt,
        Fxp,
        Fxpl,
        Xdp,
        Xfdf,
        Ahead,
        Azf,
        Azs,
        Azw,
        Acc,
        Ami,
        Apk,
        Cii,
        Fti,
        Atx,
        Mpkg,
        M3u8,
        Swi,
        Iota,
        Aep,
        Mpm,
        Bmi,
        Rep,
        Cdxml,
        Mmd,
        Cdy,
        Cla,
        Rp9,
        C4g,
        C4d,
        C4f,
        C4p,
        C4u,
        C11Amc, // C11amc
        C11Amz, // C11amz
        Csp,
        Cdbcmsg,
        Cmc,
        Clkx,
        Clkk,
        Clkp,
        Clkt,
        Clkw,
        Wbs,
        Pml,
        Ppd,
        Car,
        Pcurl,
        Dart,
        Rdz,
        Uvf,
        Uvvf,
        Uvd,
        Uvvd,
        Uvt,
        Uvvt,
        Uvx,
        Uvvx,
        Uvz,
        Uvvz,
        Fe_Launch, // fe_launch
        Dna,
        Mlp,
        Dpg,
        Dfac,
        Kpxx,
        Ait,
        Svc,
        Geo,
        Mag,
        Nml,
        Esf,
        Msf,
        Qam,
        Slt,
        Ssf,
        Es3,
        Et3,
        Ez2,
        Ez3,
        Fdf,
        Mseed,
        Seed,
        Dataless,
        Gph,
        Ftc,
        Fm,
        Frame,
        Maker,
        Book,
        Fnc,
        Ltf,
        Fsc,
        Oas,
        Oa2,
        Oa3,
        Fg5,
        Bh2,
        Ddd,
        Xdw,
        Xbd,
        Fzs,
        Txd,
        Ggb,
        Ggs,
        Ggt,
        Gex,
        Gre,
        Gxt,
        G2w,
        G3w,
        Gmx,
        Kml,
        Kmz,
        Gqf,
        Gqs,
        Gac,
        Ghf,
        Gim,
        Grv,
        Gtm,
        Tpl,
        Vcg,
        Hal,
        Zmm,
        Hbci,
        Les,
        Hpgl,
        Hpid,
        Hps,
        Jlt,
        Pcl,
        Pclxl,
        SfdHdstx, // sfd-hdstx
        Mpy,
        Afp,
        ListAfp, // listafp
        List3820, // list3820
        Irm,
        Sc,
        Icc,
        Icm,
        Igl,
        Ivp,
        Ivu,
        Igm,
        Xpw,
        Xpx,
        I2g,
        Qbo,
        Qfx,
        Rcprofile, // rcprofile
        Irp,
        Xpr,
        Fcs,
        Jam,
        Rms,
        Jisp,
        Joda,
        Ktz,
        Ktr,
        Karbon,
        Chrt,
        Kfo,
        Flw,
        Kon,
        Kpr,
        Kpt,
        Ksp,
        Kwd,
        Kwt,
        Htke,
        Kia,
        Kne,
        Knp,
        Skp,
        Skd,
        Skt,
        Skm,
        Sse,
        Lasxml, // lasxml
        Lbd,
        Lbe,
        Num123, // 123
        Apr,
        Pre,
        Nsf,
        Org,
        Scm,
        Lwp,
        Portpkg, // portpkg
        Mcd,
        Mc1,
        Cdkey, // cdkey
        Mwf,
        Mfm,
        Flo,
        Igx,
        Mif,
        Daf,
        Dis,
        Mbk,
        Mqy,
        Msl,
        Plc,
        Txf,
        Mpn,
        Mpc,
        Xul,
        Cil,
        Cab,
        Xls,
        Xlm,
        Xla,
        Xlc,
        Xlt,
        Xlw,
        Xlam,
        Xlsb,
        Xlsm,
        Xltm,
        Eot,
        Chm,
        Ims,
        Lrm,
        Thmx,
        Cat,
        Stl,
        Ppt,
        Pps,
        Pot,
        Ppam,
        Pptm,
        Sldm,
        Ppsm,
        Potm,
        Mpp,
        Mpt,
        Docm,
        Dotm,
        Wps,
        Wks,
        Wcm,
        Wdb,
        Wpl,
        Xps,
        Mseq,
        Mus,
        Msty,
        Taglet,
        Nlu,
        Ntf,
        Nitf,
        Nnd,
        Nns,
        Nnw,
        Ngdat, // ngdat
        NGage, // n-gage
        Rpst,
        Rpss,
        Edm,
        Edx,
        Ext,
        Odc,
        Otc,
        Odb,
        Odf,
        Odft,
        Odg,
        Otg,
        Odi,
        Oti,
        Odp,
        Otp,
        Ods,
        Ots,
        Odt,
        Odm,
        Ott,
        Oth,
        Xo,
        Dd2,
        Oxt,
        Pptx,
        Sldx,
        Ppsx,
        Potx,
        Xlsx,
        Xltx,
        Docx,
        Dotx,
        Mgp,
        Dp,
        Esa,
        Pdb,
        Pqa,
        Oprc,
        Paw,
        Str,
        Ei6,
        Efif,
        Wg,
        Plf,
        Pbd,
        Box,
        Mgz,
        Qps,
        Ptid,
        Qxd,
        Qxt,
        Qwd,
        Qwt,
        Qxl,
        Qxb,
        Bed,
        Mxl,
        MusicXml, // musicxml
        Cryptonote, // cryptonote
        Cod,
        Rm,
        Rmvb,
        Link66, // link66
        St,
        See,
        Sema,
        Semd,
        Semf,
        Ifm,
        Itp,
        Iif,
        Ipk,
        Twd,
        Twds,
        Mmf,
        Teacher,
        Sdkm,
        Sdkd,
        Dxp,
        Sfs,
        Sdc,
        Sda,
        Sdd,
        Smf,
        Sdw,
        Vor,
        Sgl,
        Smzip,
        Sm,
        Sxc,
        Stc,
        Sxd,
        Std,
        Sxi,
        Sti,
        Sxm,
        Sxw,
        Sxg,
        Stw,
        Sus,
        Susp,
        Svd,
        Sis,
        Sisx,
        Xsm,
        Bdm,
        Xdm,
        Tao,
        Pcap,
        Cap,
        Dmp,
        Tmo,
        Tpt,
        Mxs,
        Tra,
        Ufd,
        Ufdl,
        Utz,
        Umj,
        UnityWeb, // unityweb
        Uoml,
        Vcx,
        Vsd,
        Vst,
        Vss,
        Vsw,
        Vis,
        Vsf,
        Wbxml,
        Wmlc,
        Wmlsc,
        Wtb,
        Nbp,
        Wpd,
        Wqd,
        Stf,
        Xar,
        Xfdl,
        Hvd,
        Hvs,
        Hvp,
        Osf,
        Osfpvg,
        Saf,
        Spf,
        Cmp,
        Zir,
        Zirz,
        Zaz,
        Vxml,
        Wasm,
        Wgt,
        Hlp,
        Wsdl,
        Wspolicy, // wspolicy
        SevenZip, // 7z
        Abw,
        Ace,
        Dmg,
        Aab,
        X32,
        U32,
        Vox,
        Aam,
        Aas,
        Bcpio,
        Torrent,
        Blb,
        Blorb,
        Bz,
        Bz2,
        Boz,
        Cbr,
        Cba,
        Cbt,
        Cbz,
        Cb7,
        Vcd,
        Cfs
    }

    // --- Extension Method Class ---
    public static class MimeTypeExtensions
    {
        private static readonly ImmutableDictionary<MimeTypeName, string> MimeTypeMap = new Dictionary<MimeTypeName, string>
        {
            // Application Mime Types
            [MimeTypeName.Ez] = "application/andrew-inset",
            [MimeTypeName.Aw] = "application/applixware",
            [MimeTypeName.Atom] = "application/atom+xml",
            [MimeTypeName.AtomCat] = "application/atomcat+xml",
            [MimeTypeName.AtomSvc] = "application/atomsvc+xml",
            [MimeTypeName.Ccxml] = "application/ccxml+xml",
            [MimeTypeName.Cdmia] = "application/cdmi-capability",
            [MimeTypeName.Cdmic] = "application/cdmi-container",
            [MimeTypeName.Cdmid] = "application/cdmi-domain",
            [MimeTypeName.Cdmio] = "application/cdmi-object",
            [MimeTypeName.Cdmiq] = "application/cdmi-queue",
            [MimeTypeName.Cu] = "application/cu-seeme",
            [MimeTypeName.Davmount] = "application/davmount+xml",
            [MimeTypeName.Dbk] = "application/docbook+xml",
            [MimeTypeName.Dssc] = "application/dssc+der",
            [MimeTypeName.Xdssc] = "application/dssc+xml",
            [MimeTypeName.Ecma] = "application/ecmascript",
            [MimeTypeName.Emma] = "application/emma+xml",
            [MimeTypeName.Epub] = "application/epub+zip",
            [MimeTypeName.Exi] = "application/exi",
            [MimeTypeName.Pfr] = "application/font-tdpfr",
            [MimeTypeName.Gml] = "application/gml+xml",
            [MimeTypeName.Gpx] = "application/gpx+xml",
            [MimeTypeName.Gxf] = "application/gxf",
            [MimeTypeName.Stk] = "application/hyperstudio",
            [MimeTypeName.Ink] = "application/inkml+xml",
            [MimeTypeName.Inkml] = "application/inkml+xml",
            [MimeTypeName.Ipfix] = "application/ipfix",
            [MimeTypeName.Jar] = "application/java-archive",
            [MimeTypeName.Ser] = "application/java-serialized-object",
            [MimeTypeName.Class] = "application/java-vm",
            [MimeTypeName.Json] = "application/json",
            [MimeTypeName.Jsonml] = "application/jsonml+json",
            [MimeTypeName.LostXml] = "application/lost+xml",
            [MimeTypeName.Hqx] = "application/mac-binhex40",
            [MimeTypeName.Cpt] = "application/mac-compactpro",
            [MimeTypeName.Mads] = "application/mads+xml",
            [MimeTypeName.Mrc] = "application/marc",
            [MimeTypeName.Mrcx] = "application/marcxml+xml",
            [MimeTypeName.Ma] = "application/mathematica",
            [MimeTypeName.Nb] = "application/mathematica",
            [MimeTypeName.Mb] = "application/mathematica",
            [MimeTypeName.Mathml] = "application/mathml+xml",
            [MimeTypeName.Mbox] = "application/mbox",
            [MimeTypeName.Mscml] = "application/mediaservercontrol+xml",
            [MimeTypeName.Metalink] = "application/metalink+xml",
            [MimeTypeName.Meta4] = "application/metalink4+xml",
            [MimeTypeName.Mets] = "application/mets+xml",
            [MimeTypeName.Mods] = "application/mods+xml",
            [MimeTypeName.M21] = "application/mp21",
            [MimeTypeName.Mp21] = "application/mp21",
            [MimeTypeName.Mp4s] = "application/mp4",
            [MimeTypeName.Doc] = "application/msword",
            [MimeTypeName.Dot] = "application/msword",
            [MimeTypeName.Mxf] = "application/mxf",
            [MimeTypeName.Bin] = "application/octet-stream",
            [MimeTypeName.Dms] = "application/octet-stream",
            [MimeTypeName.Lrf] = "application/octet-stream",
            [MimeTypeName.Mar] = "application/octet-stream",
            [MimeTypeName.So] = "application/octet-stream",
            [MimeTypeName.Dist] = "application/octet-stream",
            [MimeTypeName.Distz] = "application/octet-stream",
            [MimeTypeName.Pkg] = "application/octet-stream",
            [MimeTypeName.Bpk] = "application/octet-stream",
            [MimeTypeName.Dump] = "application/octet-stream",
            [MimeTypeName.Elc] = "application/octet-stream",
            [MimeTypeName.Deploy] = "application/octet-stream",
            [MimeTypeName.Oda] = "application/oda",
            [MimeTypeName.Opf] = "application/oebps-package+xml",
            [MimeTypeName.Ogx] = "application/ogg",
            [MimeTypeName.Omdoc] = "application/omdoc+xml",
            [MimeTypeName.Onetoc] = "application/onenote",
            [MimeTypeName.Onetoc2] = "application/onenote",
            [MimeTypeName.Onetmp] = "application/onenote",
            [MimeTypeName.Onepkg] = "application/onenote",
            [MimeTypeName.Oxps] = "application/oxps",
            [MimeTypeName.Xer] = "application/patch-ops-error+xml",
            [MimeTypeName.Pdf] = "application/pdf",
            [MimeTypeName.Pgp] = "application/pgp-encrypted",
            [MimeTypeName.Asc] = "application/pgp-signature",
            [MimeTypeName.Sig] = "application/pgp-signature",
            [MimeTypeName.Prf] = "application/pics-rules",
            [MimeTypeName.P10] = "application/pkcs10",
            [MimeTypeName.P7m] = "application/pkcs7-mime",
            [MimeTypeName.P7c] = "application/pkcs7-mime",
            [MimeTypeName.P7s] = "application/pkcs7-signature",
            [MimeTypeName.P8] = "application/pkcs8",
            [MimeTypeName.Ac] = "application/pkix-attr-cert",
            [MimeTypeName.Cer] = "application/pkix-cert",
            [MimeTypeName.Crl] = "application/pkix-crl",
            [MimeTypeName.Pkipath] = "application/pkix-pkipath",
            [MimeTypeName.Pki] = "application/pkixcmp",
            [MimeTypeName.Pls] = "application/pls+xml",
            [MimeTypeName.Ai] = "application/postscript",
            [MimeTypeName.Eps] = "application/postscript",
            [MimeTypeName.Ps] = "application/postscript",
            [MimeTypeName.Cww] = "application/prs.cww",
            [MimeTypeName.Pskcxml] = "application/pskc+xml",
            [MimeTypeName.Rdf] = "application/rdf+xml",
            [MimeTypeName.Rif] = "application/reginfo+xml",
            [MimeTypeName.Rnc] = "application/relax-ng-compact-syntax",
            [MimeTypeName.Rl] = "application/resource-lists+xml",
            [MimeTypeName.Rld] = "application/resource-lists-diff+xml",
            [MimeTypeName.Rs] = "application/rls-services+xml",
            [MimeTypeName.Gbr] = "application/rpki-ghostbusters",
            [MimeTypeName.Mft] = "application/rpki-manifest",
            [MimeTypeName.Roa] = "application/rpki-roa",
            [MimeTypeName.Rsd] = "application/rsd+xml",
            [MimeTypeName.Rss] = "application/rss+xml",
            [MimeTypeName.Rtf] = "application/rtf",
            [MimeTypeName.Sbml] = "application/sbml+xml",
            [MimeTypeName.Scq] = "application/scvp-cv-request",
            [MimeTypeName.Scs] = "application/scvp-cv-response",
            [MimeTypeName.Spq] = "application/scvp-vp-request",
            [MimeTypeName.Spp] = "application/scvp-vp-response",
            [MimeTypeName.Sdp] = "application/sdp",
            [MimeTypeName.Setpay] = "application/set-payment-initiation",
            [MimeTypeName.Setreg] = "application/set-registration-initiation",
            [MimeTypeName.Shf] = "application/shf+xml",
            [MimeTypeName.Smi] = "application/smil+xml",
            [MimeTypeName.Smil] = "application/smil+xml",
            [MimeTypeName.Rq] = "application/sparql-query",
            [MimeTypeName.Srx] = "application/sparql-results+xml",
            [MimeTypeName.Gram] = "application/srgs",
            [MimeTypeName.Grxml] = "application/srgs+xml",
            [MimeTypeName.Sru] = "application/sru+xml",
            [MimeTypeName.Ssdl] = "application/ssdl+xml",
            [MimeTypeName.Ssml] = "application/ssml+xml",
            [MimeTypeName.Tei] = "application/tei+xml",
            [MimeTypeName.Teicorpus] = "application/tei+xml",
            [MimeTypeName.Tfi] = "application/thraud+xml",
            [MimeTypeName.Tsd] = "application/timestamped-data",
            [MimeTypeName.Plb] = "application/vnd.3gpp.pic-bw-large",
            [MimeTypeName.Psb] = "application/vnd.3gpp.pic-bw-small",
            [MimeTypeName.Pvb] = "application/vnd.3gpp.pic-bw-var",
            [MimeTypeName.Tcap] = "application/vnd.3gpp2.tcap",
            [MimeTypeName.Pwn] = "application/vnd.3m.post-it-notes",
            [MimeTypeName.Aso] = "application/vnd.accpac.simply.aso",
            [MimeTypeName.Imp] = "application/vnd.accpac.simply.imp",
            [MimeTypeName.Acu] = "application/vnd.acucobol",
            [MimeTypeName.Atc] = "application/vnd.acucorp",
            [MimeTypeName.Acutc] = "application/vnd.acucorp",
            [MimeTypeName.Air] = "application/vnd.adobe.air-application-installer-package+zip",
            [MimeTypeName.Fcdt] = "application/vnd.adobe.formscentral.fcdt",
            [MimeTypeName.Fxp] = "application/vnd.adobe.fxp",
            [MimeTypeName.Fxpl] = "application/vnd.adobe.fxp",
            [MimeTypeName.Xdp] = "application/vnd.adobe.xdp+xml",
            [MimeTypeName.Xfdf] = "application/vnd.adobe.xfdf",
            [MimeTypeName.Ahead] = "application/vnd.ahead.space",
            [MimeTypeName.Azf] = "application/vnd.airzip.filesecure.azf",
            [MimeTypeName.Azs] = "application/vnd.airzip.filesecure.azs",
            [MimeTypeName.Azw] = "application/vnd.amazon.ebook",
            [MimeTypeName.Acc] = "application/vnd.americandynamics.acc",
            [MimeTypeName.Ami] = "application/vnd.amiga.ami",
            [MimeTypeName.Apk] = "application/vnd.android.package-archive",
            [MimeTypeName.Cii] = "application/vnd.anser-web-certificate-issue-initiation",
            [MimeTypeName.Fti] = "application/vnd.anser-web-funds-transfer-initiation",
            [MimeTypeName.Atx] = "application/vnd.antix.game-component",
            [MimeTypeName.Mpkg] = "application/vnd.apple.installer+xml",
            [MimeTypeName.M3u8] = "application/vnd.apple.mpegurl",
            [MimeTypeName.Swi] = "application/vnd.aristanetworks.swi",
            [MimeTypeName.Iota] = "application/vnd.astraea-software.iota",
            [MimeTypeName.Aep] = "application/vnd.audiograph",
            [MimeTypeName.Mpm] = "application/vnd.blueice.multipass",
            [MimeTypeName.Bmi] = "application/vnd.bmi",
            [MimeTypeName.Rep] = "application/vnd.businessobjects",
            [MimeTypeName.Cdxml] = "application/vnd.chemdraw+xml",
            [MimeTypeName.Mmd] = "application/vnd.chipnuts.karaoke-mmd",
            [MimeTypeName.Cdy] = "application/vnd.cinderella",
            [MimeTypeName.Cla] = "application/vnd.claymore",
            [MimeTypeName.Rp9] = "application/vnd.cloanto.rp9",
            [MimeTypeName.C4g] = "application/vnd.clonk.c4group",
            [MimeTypeName.C4d] = "application/vnd.clonk.c4group",
            [MimeTypeName.C4f] = "application/vnd.clonk.c4group",
            [MimeTypeName.C4p] = "application/vnd.clonk.c4group",
            [MimeTypeName.C4u] = "application/vnd.clonk.c4group",
            [MimeTypeName.C11Amc] = "application/vnd.cluetrust.cartomobile-config",
            [MimeTypeName.C11Amz] = "application/vnd.cluetrust.cartomobile-config-pkg",
            [MimeTypeName.Csp] = "application/vnd.commonspace",
            [MimeTypeName.Cdbcmsg] = "application/vnd.contact.cmsg",
            [MimeTypeName.Cmc] = "application/vnd.cosmocaller",
            [MimeTypeName.Clkx] = "application/vnd.crick.clicker",
            [MimeTypeName.Clkk] = "application/vnd.crick.clicker.keyboard",
            [MimeTypeName.Clkp] = "application/vnd.crick.clicker.palette",
            [MimeTypeName.Clkt] = "application/vnd.crick.clicker.template",
            [MimeTypeName.Clkw] = "application/vnd.crick.clicker.wordbank",
            [MimeTypeName.Wbs] = "application/vnd.criticaltools.wbs+xml",
            [MimeTypeName.Pml] = "application/vnd.ctc-posml",
            [MimeTypeName.Ppd] = "application/vnd.cups-ppd",
            [MimeTypeName.Car] = "application/vnd.curl.car",
            [MimeTypeName.Pcurl] = "application/vnd.curl.pcurl",
            [MimeTypeName.Dart] = "application/vnd.dart",
            [MimeTypeName.Rdz] = "application/vnd.data-vision.rdz",
            [MimeTypeName.Uvf] = "application/vnd.dece.data",
            [MimeTypeName.Uvvf] = "application/vnd.dece.data",
            [MimeTypeName.Uvd] = "application/vnd.dece.data",
            [MimeTypeName.Uvvd] = "application/vnd.dece.data",
            [MimeTypeName.Uvt] = "application/vnd.dece.ttml+xml",
            [MimeTypeName.Uvvt] = "application/vnd.dece.ttml+xml",
            [MimeTypeName.Uvx] = "application/vnd.dece.unspecified",
            [MimeTypeName.Uvvx] = "application/vnd.dece.unspecified",
            [MimeTypeName.Uvz] = "application/vnd.dece.zip",
            [MimeTypeName.Uvvz] = "application/vnd.dece.zip",
            [MimeTypeName.Fe_Launch] = "application/vnd.denovo.fcselayout-link",
            [MimeTypeName.Dna] = "application/vnd.dna",
            [MimeTypeName.Mlp] = "application/vnd.dolby.mlp",
            [MimeTypeName.Dpg] = "application/vnd.dpgraph",
            [MimeTypeName.Dfac] = "application/vnd.dreamfactory",
            [MimeTypeName.Kpxx] = "application/vnd.ds-keypoint",
            [MimeTypeName.Ait] = "application/vnd.dvb.ait",
            [MimeTypeName.Svc] = "application/vnd.dvb.service",
            [MimeTypeName.Geo] = "application/vnd.dynageo",
            [MimeTypeName.Mag] = "application/vnd.ecowin.chart",
            [MimeTypeName.Nml] = "application/vnd.enliven",
            [MimeTypeName.Esf] = "application/vnd.epson.esf",
            [MimeTypeName.Msf] = "application/vnd.epson.msf",
            [MimeTypeName.Qam] = "application/vnd.epson.quickanime",
            [MimeTypeName.Slt] = "application/vnd.epson.salt",
            [MimeTypeName.Ssf] = "application/vnd.epson.ssf",
            [MimeTypeName.Es3] = "application/vnd.eszigno3+xml",
            [MimeTypeName.Et3] = "application/vnd.eszigno3+xml",
            [MimeTypeName.Ez2] = "application/vnd.ezpix-album",
            [MimeTypeName.Ez3] = "application/vnd.ezpix-package",
            [MimeTypeName.Fdf] = "application/vnd.fdf",
            [MimeTypeName.Mseed] = "application/vnd.fdsn.mseed",
            [MimeTypeName.Seed] = "application/vnd.fdsn.seed",
            [MimeTypeName.Dataless] = "application/vnd.fdsn.seed",
            [MimeTypeName.Gph] = "application/vnd.flographit",
            [MimeTypeName.Ftc] = "application/vnd.fluxtime.clip",
            [MimeTypeName.Fm] = "application/vnd.framemaker",
            [MimeTypeName.Frame] = "application/vnd.framemaker",
            [MimeTypeName.Maker] = "application/vnd.framemaker",
            [MimeTypeName.Book] = "application/vnd.framemaker",
            [MimeTypeName.Fnc] = "application/vnd.frogans.fnc",
            [MimeTypeName.Ltf] = "application/vnd.frogans.ltf",
            [MimeTypeName.Fsc] = "application/vnd.fsc.weblaunch",
            [MimeTypeName.Oas] = "application/vnd.fujitsu.oasys",
            [MimeTypeName.Oa2] = "application/vnd.fujitsu.oasys2",
            [MimeTypeName.Oa3] = "application/vnd.fujitsu.oasys3",
            [MimeTypeName.Fg5] = "application/vnd.fujitsu.oasysgp",
            [MimeTypeName.Bh2] = "application/vnd.fujitsu.oasysprs",
            [MimeTypeName.Ddd] = "application/vnd.fujixerox.ddd",
            [MimeTypeName.Xdw] = "application/vnd.fujixerox.docuworks",
            [MimeTypeName.Xbd] = "application/vnd.fujixerox.docuworks.binder",
            [MimeTypeName.Fzs] = "application/vnd.fuzzysheet",
            [MimeTypeName.Txd] = "application/vnd.genomatix.tuxedo",
            [MimeTypeName.Ggb] = "application/vnd.geogebra.file",
            [MimeTypeName.Ggs] = "application/vnd.geogebra.slides",
            [MimeTypeName.Ggt] = "application/vnd.geogebra.tool",
            [MimeTypeName.Gex] = "application/vnd.geometry-explorer",
            [MimeTypeName.Gre] = "application/vnd.geometry-explorer",
            [MimeTypeName.Gxt] = "application/vnd.geonext",
            [MimeTypeName.G2w] = "application/vnd.geoplan",
            [MimeTypeName.G3w] = "application/vnd.geospace",
            [MimeTypeName.Gmx] = "application/vnd.gmx",
            [MimeTypeName.Kml] = "application/vnd.google-earth.kml+xml",
            [MimeTypeName.Kmz] = "application/vnd.google-earth.kmz",
            [MimeTypeName.Gqf] = "application/vnd.grafeq",
            [MimeTypeName.Gqs] = "application/vnd.grafeq",
            [MimeTypeName.Gac] = "application/vnd.groove-account",
            [MimeTypeName.Ghf] = "application/vnd.groove-help",
            [MimeTypeName.Gim] = "application/vnd.groove-identity-message",
            [MimeTypeName.Grv] = "application/vnd.groove-injector",
            [MimeTypeName.Gtm] = "application/vnd.groove-tool-message",
            [MimeTypeName.Tpl] = "application/vnd.groove-tool-template",
            [MimeTypeName.Vcg] = "application/vnd.groove-vcard",
            [MimeTypeName.Hal] = "application/vnd.hal+xml",
            [MimeTypeName.Zmm] = "application/vnd.handheld-entertainment+xml",
            [MimeTypeName.Hbci] = "application/vnd.hbci",
            [MimeTypeName.Les] = "application/vnd.hhe.lesson-player",
            [MimeTypeName.Hpgl] = "application/vnd.hp-hpgl",
            [MimeTypeName.Hpid] = "application/vnd.hp-hpid",
            [MimeTypeName.Hps] = "application/vnd.hp-hps",
            [MimeTypeName.Jlt] = "application/vnd.hp-jlyt",
            [MimeTypeName.Pcl] = "application/vnd.hp-pcl",
            [MimeTypeName.Pclxl] = "application/vnd.hp-pclxl",
            [MimeTypeName.SfdHdstx] = "application/vnd.hydrostatix.sof-data",
            [MimeTypeName.Mpy] = "application/vnd.ibm.minipay",
            [MimeTypeName.Afp] = "application/vnd.ibm.modcap",
            [MimeTypeName.ListAfp] = "application/vnd.ibm.modcap",
            [MimeTypeName.List3820] = "application/vnd.ibm.modcap",
            [MimeTypeName.Irm] = "application/vnd.ibm.rights-management",
            [MimeTypeName.Sc] = "application/vnd.ibm.secure-container",
            [MimeTypeName.Icc] = "application/vnd.iccprofile",
            [MimeTypeName.Icm] = "application/vnd.iccprofile",
            [MimeTypeName.Igl] = "application/vnd.igloader",
            [MimeTypeName.Ivp] = "application/vnd.immervision-ivp",
            [MimeTypeName.Ivu] = "application/vnd.immervision-ivu",
            [MimeTypeName.Igm] = "application/vnd.insors.igm",
            [MimeTypeName.Xpw] = "application/vnd.intercon.formnet",
            [MimeTypeName.Xpx] = "application/vnd.intercon.formnet",
            [MimeTypeName.I2g] = "application/vnd.intergeo",
            [MimeTypeName.Qbo] = "application/vnd.intu.qbo",
            [MimeTypeName.Qfx] = "application/vnd.intu.qfx",
            [MimeTypeName.Rcprofile] = "application/vnd.ipunplugged.rcprofile",
            [MimeTypeName.Irp] = "application/vnd.irepository.package+xml",
            [MimeTypeName.Xpr] = "application/vnd.is-xpr",
            [MimeTypeName.Fcs] = "application/vnd.isac.fcs",
            [MimeTypeName.Jam] = "application/vnd.jam",
            [MimeTypeName.Rms] = "application/vnd.jcp.javame.midlet-rms",
            [MimeTypeName.Jisp] = "application/vnd.jisp",
            [MimeTypeName.Joda] = "application/vnd.joost.joda-archive",
            [MimeTypeName.Ktz] = "application/vnd.kahootz",
            [MimeTypeName.Ktr] = "application/vnd.kahootz",
            [MimeTypeName.Karbon] = "application/vnd.kde.karbon",
            [MimeTypeName.Chrt] = "application/vnd.kde.kchart",
            [MimeTypeName.Kfo] = "application/vnd.kde.kformula",
            [MimeTypeName.Flw] = "application/vnd.kde.kivio",
            [MimeTypeName.Kon] = "application/vnd.kde.kontour",
            [MimeTypeName.Kpr] = "application/vnd.kde.kpresenter",
            [MimeTypeName.Kpt] = "application/vnd.kde.kpresenter",
            [MimeTypeName.Ksp] = "application/vnd.kde.kspread",
            [MimeTypeName.Kwd] = "application/vnd.kde.kword",
            [MimeTypeName.Kwt] = "application/vnd.kde.kword",
            [MimeTypeName.Htke] = "application/vnd.kenameaapp",
            [MimeTypeName.Kia] = "application/vnd.kidspiration",
            [MimeTypeName.Kne] = "application/vnd.kinar",
            [MimeTypeName.Knp] = "application/vnd.kinar",
            [MimeTypeName.Skp] = "application/vnd.koan",
            [MimeTypeName.Skd] = "application/vnd.koan",
            [MimeTypeName.Skt] = "application/vnd.koan",
            [MimeTypeName.Skm] = "application/vnd.koan",
            [MimeTypeName.Sse] = "application/vnd.kodak-descriptor",
            [MimeTypeName.Lasxml] = "application/vnd.las.las+xml",
            [MimeTypeName.Lbd] = "application/vnd.llamagraphics.life-balance.desktop",
            [MimeTypeName.Lbe] = "application/vnd.llamagraphics.life-balance.exchange+xml",
            [MimeTypeName.Num123] = "application/vnd.lotus-1-2-3",
            [MimeTypeName.Apr] = "application/vnd.lotus-approach",
            [MimeTypeName.Pre] = "application/vnd.lotus-freelance",
            [MimeTypeName.Nsf] = "application/vnd.lotus-notes",
            [MimeTypeName.Org] = "application/vnd.lotus-organizer",
            [MimeTypeName.Scm] = "application/vnd.lotus-screencam",
            [MimeTypeName.Lwp] = "application/vnd.lotus-wordpro",
            [MimeTypeName.Portpkg] = "application/vnd.macports.portpkg",
            [MimeTypeName.Mcd] = "application/vnd.mcd",
            [MimeTypeName.Mc1] = "application/vnd.medcalcdata",
            [MimeTypeName.Cdkey] = "application/vnd.mediastation.cdkey",
            [MimeTypeName.Mwf] = "application/vnd.mfer",
            [MimeTypeName.Mfm] = "application/vnd.mfmp",
            [MimeTypeName.Flo] = "application/vnd.micrografx.flo",
            [MimeTypeName.Igx] = "application/vnd.micrografx.igx",
            [MimeTypeName.Mif] = "application/vnd.mif",
            [MimeTypeName.Daf] = "application/vnd.mobius.daf",
            [MimeTypeName.Dis] = "application/vnd.mobius.dis",
            [MimeTypeName.Mbk] = "application/vnd.mobius.mbk",
            [MimeTypeName.Mqy] = "application/vnd.mobius.mqy",
            [MimeTypeName.Msl] = "application/vnd.mobius.msl",
            [MimeTypeName.Plc] = "application/vnd.mobius.plc",
            [MimeTypeName.Txf] = "application/vnd.mobius.txf",
            [MimeTypeName.Mpn] = "application/vnd.mophun.application",
            [MimeTypeName.Mpc] = "application/vnd.mophun.certificate",
            [MimeTypeName.Xul] = "application/vnd.mozilla.xul+xml",
            [MimeTypeName.Cil] = "application/vnd.ms-artgalry",
            [MimeTypeName.Cab] = "application/vnd.ms-cab-compressed",
            [MimeTypeName.Xls] = "application/vnd.ms-excel",
            [MimeTypeName.Xlm] = "application/vnd.ms-excel",
            [MimeTypeName.Xla] = "application/vnd.ms-excel",
            [MimeTypeName.Xlc] = "application/vnd.ms-excel",
            [MimeTypeName.Xlt] = "application/vnd.ms-excel",
            [MimeTypeName.Xlw] = "application/vnd.ms-excel",
            [MimeTypeName.Xlam] = "application/vnd.ms-excel.addin.macroenabled.12",
            [MimeTypeName.Xlsb] = "application/vnd.ms-excel.sheet.binary.macroenabled.12",
            [MimeTypeName.Xlsm] = "application/vnd.ms-excel.sheet.macroenabled.12",
            [MimeTypeName.Xltm] = "application/vnd.ms-excel.template.macroenabled.12",
            [MimeTypeName.Eot] = "application/vnd.ms-fontobject",
            [MimeTypeName.Chm] = "application/vnd.ms-htmlhelp",
            [MimeTypeName.Ims] = "application/vnd.ms-ims",
            [MimeTypeName.Lrm] = "application/vnd.ms-lrm",
            [MimeTypeName.Thmx] = "application/vnd.ms-officetheme",
            [MimeTypeName.Cat] = "application/vnd.ms-pki.seccat",
            [MimeTypeName.Stl] = "application/vnd.ms-pki.stl",
            [MimeTypeName.Ppt] = "application/vnd.ms-powerpoint",
            [MimeTypeName.Pps] = "application/vnd.ms-powerpoint",
            [MimeTypeName.Pot] = "application/vnd.ms-powerpoint",
            [MimeTypeName.Ppam] = "application/vnd.ms-powerpoint.addin.macroenabled.12",
            [MimeTypeName.Pptm] = "application/vnd.ms-powerpoint.presentation.macroenabled.12",
            [MimeTypeName.Sldm] = "application/vnd.ms-powerpoint.slide.macroenabled.12",
            [MimeTypeName.Ppsm] = "application/vnd.ms-powerpoint.slideshow.macroenabled.12",
            [MimeTypeName.Potm] = "application/vnd.ms-powerpoint.template.macroenabled.12",
            [MimeTypeName.Mpp] = "application/vnd.ms-project",
            [MimeTypeName.Mpt] = "application/vnd.ms-project",
            [MimeTypeName.Docm] = "application/vnd.ms-word.document.macroenabled.12",
            [MimeTypeName.Dotm] = "application/vnd.ms-word.template.macroenabled.12",
            [MimeTypeName.Wps] = "application/vnd.ms-works",
            [MimeTypeName.Wks] = "application/vnd.ms-works",
            [MimeTypeName.Wcm] = "application/vnd.ms-works",
            [MimeTypeName.Wdb] = "application/vnd.ms-works",
            [MimeTypeName.Wpl] = "application/vnd.ms-wpl",
            [MimeTypeName.Xps] = "application/vnd.ms-xpsdocument",
            [MimeTypeName.Mseq] = "application/vnd.mseq",
            [MimeTypeName.Mus] = "application/vnd.musician",
            [MimeTypeName.Msty] = "application/vnd.muvee.style",
            [MimeTypeName.Taglet] = "application/vnd.mynfc",
            [MimeTypeName.Nlu] = "application/vnd.neurolanguage.nlu",
            [MimeTypeName.Ntf] = "application/vnd.nitf",
            [MimeTypeName.Nitf] = "application/vnd.nitf",
            [MimeTypeName.Nnd] = "application/vnd.noblenet-directory",
            [MimeTypeName.Nns] = "application/vnd.noblenet-sealer",
            [MimeTypeName.Nnw] = "application/vnd.noblenet-web",
            [MimeTypeName.Ngdat] = "application/vnd.nokia.n-gage.data",
            [MimeTypeName.NGage] = "application/vnd.nokia.n-gage.symbian.install",
            [MimeTypeName.Rpst] = "application/vnd.nokia.radio-preset",
            [MimeTypeName.Rpss] = "application/vnd.nokia.radio-presets",
            [MimeTypeName.Edm] = "application/vnd.novadigm.edm",
            [MimeTypeName.Edx] = "application/vnd.novadigm.edx",
            [MimeTypeName.Ext] = "application/vnd.novadigm.ext",
            [MimeTypeName.Odc] = "application/vnd.oasis.opendocument.chart",
            [MimeTypeName.Otc] = "application/vnd.oasis.opendocument.chart-template",
            [MimeTypeName.Odb] = "application/vnd.oasis.opendocument.database",
            [MimeTypeName.Odf] = "application/vnd.oasis.opendocument.formula",
            [MimeTypeName.Odft] = "application/vnd.oasis.opendocument.formula-template",
            [MimeTypeName.Odg] = "application/vnd.oasis.opendocument.graphics",
            [MimeTypeName.Otg] = "application/vnd.oasis.opendocument.graphics-template",
            [MimeTypeName.Odi] = "application/vnd.oasis.opendocument.image",
            [MimeTypeName.Oti] = "application/vnd.oasis.opendocument.image-template",
            [MimeTypeName.Odp] = "application/vnd.oasis.opendocument.presentation",
            [MimeTypeName.Otp] = "application/vnd.oasis.opendocument.presentation-template",
            [MimeTypeName.Ods] = "application/vnd.oasis.opendocument.spreadsheet",
            [MimeTypeName.Ots] = "application/vnd.oasis.opendocument.spreadsheet-template",
            [MimeTypeName.Odt] = "application/vnd.oasis.opendocument.text",
            [MimeTypeName.Odm] = "application/vnd.oasis.opendocument.text-master",
            [MimeTypeName.Ott] = "application/vnd.oasis.opendocument.text-template",
            [MimeTypeName.Oth] = "application/vnd.oasis.opendocument.text-web",
            [MimeTypeName.Xo] = "application/vnd.olpc-sugar",
            [MimeTypeName.Dd2] = "application/vnd.oma.dd2+xml",
            [MimeTypeName.Oxt] = "application/vnd.openofficeorg.extension",
            [MimeTypeName.Pptx] = "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            [MimeTypeName.Sldx] = "application/vnd.openxmlformats-officedocument.presentationml.slide",
            [MimeTypeName.Ppsx] = "application/vnd.openxmlformats-officedocument.presentationml.slideshow",
            [MimeTypeName.Potx] = "application/vnd.openxmlformats-officedocument.presentationml.template",
            [MimeTypeName.Xlsx] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            [MimeTypeName.Xltx] = "application/vnd.openxmlformats-officedocument.spreadsheetml.template",
            [MimeTypeName.Docx] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            [MimeTypeName.Dotx] = "application/vnd.openxmlformats-officedocument.wordprocessingml.template",
            [MimeTypeName.Mgp] = "application/vnd.osgeo.mapguide.package",
            [MimeTypeName.Dp] = "application/vnd.osgi.dp",
            [MimeTypeName.Esa] = "application/vnd.osgi.subsystem",
            [MimeTypeName.Pdb] = "application/vnd.palm",
            [MimeTypeName.Pqa] = "application/vnd.palm",
            [MimeTypeName.Oprc] = "application/vnd.palm",
            [MimeTypeName.Paw] = "application/vnd.pawaafile",
            [MimeTypeName.Str] = "application/vnd.pg.format",
            [MimeTypeName.Ei6] = "application/vnd.pg.osasli",
            [MimeTypeName.Efif] = "application/vnd.picsel",
            [MimeTypeName.Wg] = "application/vnd.pmi.widget",
            [MimeTypeName.Plf] = "application/vnd.pocketlearn",
            [MimeTypeName.Pbd] = "application/vnd.powerbuilder6",
            [MimeTypeName.Box] = "application/vnd.previewsystems.box",
            [MimeTypeName.Mgz] = "application/vnd.proteus.magazine",
            [MimeTypeName.Qps] = "application/vnd.publishare-delta-tree",
            [MimeTypeName.Ptid] = "application/vnd.pvi.ptid1",
            [MimeTypeName.Qxd] = "application/vnd.quark.quarkxpress",
            [MimeTypeName.Qxt] = "application/vnd.quark.quarkxpress",
            [MimeTypeName.Qwd] = "application/vnd.quark.quarkxpress",
            [MimeTypeName.Qwt] = "application/vnd.quark.quarkxpress",
            [MimeTypeName.Qxl] = "application/vnd.quark.quarkxpress",
            [MimeTypeName.Qxb] = "application/vnd.quark.quarkxpress",
            [MimeTypeName.Bed] = "application/vnd.realvnc.bed",
            [MimeTypeName.Mxl] = "application/vnd.recordare.musicxml",
            [MimeTypeName.MusicXml] = "application/vnd.recordare.musicxml+xml",
            [MimeTypeName.Cryptonote] = "application/vnd.rig.cryptonote",
            [MimeTypeName.Cod] = "application/vnd.rim.cod",
            [MimeTypeName.Rm] = "application/vnd.rn-realmedia",
            [MimeTypeName.Rmvb] = "application/vnd.rn-realmedia-vbr",
            [MimeTypeName.Link66] = "application/vnd.route66.link66+xml",
            [MimeTypeName.St] = "application/vnd.sailingtracker.track",
            [MimeTypeName.See] = "application/vnd.seemail",
            [MimeTypeName.Sema] = "application/vnd.sema",
            [MimeTypeName.Semd] = "application/vnd.semd",
            [MimeTypeName.Semf] = "application/vnd.semf",
            [MimeTypeName.Ifm] = "application/vnd.shana.informed.formdata",
            [MimeTypeName.Itp] = "application/vnd.shana.informed.formtemplate",
            [MimeTypeName.Iif] = "application/vnd.shana.informed.interchange",
            [MimeTypeName.Ipk] = "application/vnd.shana.informed.package",
            [MimeTypeName.Twd] = "application/vnd.simtech-mindmapper",
            [MimeTypeName.Twds] = "application/vnd.simtech-mindmapper",
            [MimeTypeName.Mmf] = "application/vnd.smaf",
            [MimeTypeName.Teacher] = "application/vnd.smart.teacher",
            [MimeTypeName.Sdkm] = "application/vnd.solent.sdkm+xml",
            [MimeTypeName.Sdkd] = "application/vnd.solent.sdkm+xml",
            [MimeTypeName.Dxp] = "application/vnd.spotfire.dxp",
            [MimeTypeName.Sfs] = "application/vnd.spotfire.sfs",
            [MimeTypeName.Sdc] = "application/vnd.stardivision.calc",
            [MimeTypeName.Sda] = "application/vnd.stardivision.draw",
            [MimeTypeName.Sdd] = "application/vnd.stardivision.impress",
            [MimeTypeName.Smf] = "application/vnd.stardivision.math",
            [MimeTypeName.Sdw] = "application/vnd.stardivision.writer",
            [MimeTypeName.Vor] = "application/vnd.stardivision.writer",
            [MimeTypeName.Sgl] = "application/vnd.stardivision.writer-global",
            [MimeTypeName.Smzip] = "application/vnd.stepmania.package",
            [MimeTypeName.Sm] = "application/vnd.stepmania.stepchart",
            [MimeTypeName.Sxc] = "application/vnd.sun.xml.calc",
            [MimeTypeName.Stc] = "application/vnd.sun.xml.calc.template",
            [MimeTypeName.Sxd] = "application/vnd.sun.xml.draw",
            [MimeTypeName.Std] = "application/vnd.sun.xml.draw.template",
            [MimeTypeName.Sxi] = "application/vnd.sun.xml.impress",
            [MimeTypeName.Sti] = "application/vnd.sun.xml.impress.template",
            [MimeTypeName.Sxm] = "application/vnd.sun.xml.math",
            [MimeTypeName.Sxw] = "application/vnd.sun.xml.writer",
            [MimeTypeName.Sxg] = "application/vnd.sun.xml.writer.global",
            [MimeTypeName.Stw] = "application/vnd.sun.xml.writer.template",
            [MimeTypeName.Sus] = "application/vnd.sus-calendar",
            [MimeTypeName.Susp] = "application/vnd.sus-calendar",
            [MimeTypeName.Svd] = "application/vnd.svd",
            [MimeTypeName.Sis] = "application/vnd.symbian.install",
            [MimeTypeName.Sisx] = "application/vnd.symbian.install",
            [MimeTypeName.Xsm] = "application/vnd.syncml+xml",
            [MimeTypeName.Bdm] = "application/vnd.syncml.dm+wbxml",
            [MimeTypeName.Xdm] = "application/vnd.syncml.dm+xml",
            [MimeTypeName.Tao] = "application/vnd.tao.intent-module-archive",
            [MimeTypeName.Pcap] = "application/vnd.tcpdump.pcap",
            [MimeTypeName.Cap] = "application/vnd.tcpdump.pcap",
            [MimeTypeName.Dmp] = "application/vnd.tcpdump.pcap",
            [MimeTypeName.Tmo] = "application/vnd.tmobile-livetv",
            [MimeTypeName.Tpt] = "application/vnd.trid.tpt",
            [MimeTypeName.Mxs] = "application/vnd.triscape.mxs",
            [MimeTypeName.Tra] = "application/vnd.trueapp",
            [MimeTypeName.Ufd] = "application/vnd.ufdl",
            [MimeTypeName.Ufdl] = "application/vnd.ufdl",
            [MimeTypeName.Utz] = "application/vnd.uiq.theme",
            [MimeTypeName.Umj] = "application/vnd.umajin",
            [MimeTypeName.UnityWeb] = "application/vnd.unity",
            [MimeTypeName.Uoml] = "application/vnd.uoml+xml",
            [MimeTypeName.Vcx] = "application/vnd.vcx",
            [MimeTypeName.Vsd] = "application/vnd.visio",
            [MimeTypeName.Vst] = "application/vnd.visio",
            [MimeTypeName.Vss] = "application/vnd.visio",
            [MimeTypeName.Vsw] = "application/vnd.visio",
            [MimeTypeName.Vis] = "application/vnd.visionary",
            [MimeTypeName.Vsf] = "application/vnd.vsf",
            [MimeTypeName.Wbxml] = "application/vnd.wap.wbxml",
            [MimeTypeName.Wmlc] = "application/vnd.wap.wmlc",
            [MimeTypeName.Wmlsc] = "application/vnd.wap.wmlscriptc",
            [MimeTypeName.Wtb] = "application/vnd.webturbo",
            [MimeTypeName.Nbp] = "application/vnd.wolfram.player",
            [MimeTypeName.Wpd] = "application/vnd.wordperfect",
            [MimeTypeName.Wqd] = "application/vnd.wqd",
            [MimeTypeName.Stf] = "application/vnd.wt.stf",
            [MimeTypeName.Xar] = "application/vnd.xara",
            [MimeTypeName.Xfdl] = "application/vnd.xfdl",
            [MimeTypeName.Hvd] = "application/vnd.yamaha.hv-dic",
            [MimeTypeName.Hvs] = "application/vnd.yamaha.hv-script",
            [MimeTypeName.Hvp] = "application/vnd.yamaha.hv-voice",
            [MimeTypeName.Osf] = "application/vnd.yamaha.openscoreformat",
            [MimeTypeName.Osfpvg] = "application/vnd.yamaha.openscoreformat.osfpvg+xml",
            [MimeTypeName.Saf] = "application/vnd.yamaha.smaf-audio",
            [MimeTypeName.Spf] = "application/vnd.yamaha.smaf-phrase",
            [MimeTypeName.Cmp] = "application/vnd.yellowriver-custom-menu",
            [MimeTypeName.Zir] = "application/vnd.zul",
            [MimeTypeName.Zirz] = "application/vnd.zul",
            [MimeTypeName.Zaz] = "application/vnd.zzazz.deck+xml",
            [MimeTypeName.Vxml] = "application/voicexml+xml",
            [MimeTypeName.Wasm] = "application/wasm",
            [MimeTypeName.Wgt] = "application/widget",
            [MimeTypeName.Hlp] = "application/winhlp",
            [MimeTypeName.Wsdl] = "application/wsdl+xml",
            [MimeTypeName.Wspolicy] = "application/wspolicy+xml",
            [MimeTypeName.SevenZip] = "application/x-7z-compressed",
            [MimeTypeName.Abw] = "application/x-abiword",
            [MimeTypeName.Ace] = "application/x-ace-compressed",
            [MimeTypeName.Dmg] = "application/x-apple-diskimage",
            [MimeTypeName.Aab] = "application/x-authorware-bin",
            [MimeTypeName.X32] = "application/x-authorware-bin",
            [MimeTypeName.U32] = "application/x-authorware-bin",
            [MimeTypeName.Vox] = "application/x-authorware-bin",
            [MimeTypeName.Aam] = "application/x-authorware-map",
            [MimeTypeName.Aas] = "application/x-authorware-seg",
            [MimeTypeName.Bcpio] = "application/x-bcpio",
            [MimeTypeName.Torrent] = "application/x-bittorrent",
            [MimeTypeName.Blb] = "application/x-blorb",
            [MimeTypeName.Blorb] = "application/x-blorb",
            [MimeTypeName.Bz] = "application/x-bzip",
            [MimeTypeName.Bz2] = "application/x-bzip2",
            [MimeTypeName.Boz] = "application/x-bzip2",
            [MimeTypeName.Cbr] = "application/x-cbr",
            [MimeTypeName.Cba] = "application/x-cbr",
            [MimeTypeName.Cbt] = "application/x-cbr",
            [MimeTypeName.Cbz] = "application/x-cbr",
            [MimeTypeName.Cb7] = "application/x-cbr",
            [MimeTypeName.Vcd] = "application/x-cdlink",
            [MimeTypeName.Cfs] = "application/cfs"
        }.ToImmutableDictionary();

        /// <summary>
        /// Gets the MIME type string for the specified MimeTypeName enum value.
        /// </summary>
        /// <param name="mimeTypeName">The enum value representing the MIME type name.</param>
        /// <returns>The corresponding MIME type string, or null if the enum value is not found.</returns>
        public static string? MimeType(this MimeTypeName mimeTypeName)
        {
            if (MimeTypeMap.TryGetValue(mimeTypeName, out string? mimeType))
                return mimeType;

            else return null;
        }
    }
    #endregion
    #endregion
}
