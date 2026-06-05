using LearningArchitect.Modules.InterviewArena.Net;
using NUnit.Framework;

namespace LearningArchitect.Modules.InterviewArena.Tests.Editor
{
    public sealed class BackendJsonParserTests
    {
        [Test]
        public void Parses_login_response()
        {
            const string json =
                "{\"userId\":\"u_abc\",\"username\":\"Runner\",\"sessionToken\":\"tok\",\"expiresAt\":\"2026-05-25T12:00:00Z\"}";

            Assert.IsTrue(BackendJsonParser.TryDeserialize(json, out LoginByNameResponseDto dto));
            Assert.AreEqual("u_abc", dto.userId);
            Assert.AreEqual("Runner", dto.username);
            Assert.AreEqual("tok", dto.sessionToken);
        }

        [Test]
        public void Parses_lobby_list()
        {
            const string json =
                "{\"items\":[{\"lobbyId\":\"lobby_1\",\"name\":\"Forest Run\",\"playerCount\":2,\"maxPlayers\":4,\"state\":\"Preparing\"}]}";

            Assert.IsTrue(BackendJsonParser.TryDeserialize(json, out LobbyListResponseDto dto));
            Assert.NotNull(dto.items);
            Assert.AreEqual(1, dto.items.Length);
            Assert.AreEqual("lobby_1", dto.items[0].lobbyId);
        }

        [Test]
        public void Parses_api_error()
        {
            const string json =
                "{\"errorCode\":\"USERNAME_INVALID\",\"message\":\"Username must be 3-16 characters.\"}";

            Assert.IsTrue(BackendJsonParser.TryDeserialize(json, out ApiErrorDto dto));
            Assert.AreEqual("USERNAME_INVALID", dto.errorCode);
        }

        [Test]
        public void Serializes_create_lobby_request()
        {
            string json = BackendJsonParser.Serialize(new CreateLobbyRequestDto("Forest Run", 4));
            StringAssert.Contains("Forest Run", json);
            StringAssert.Contains("4", json);
        }
    }
}
