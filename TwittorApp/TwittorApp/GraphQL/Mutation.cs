using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TwittorApp.Dtos;
using TwittorApp.Kafka;
using TwittorApp.Models;

namespace TwittorApp.GraphQL
{
    public class Mutation
    {
        [Authorize(Roles = new[] { "MEMBER"})]
        public async Task<TransactionStatus> AddTwitAsync(
           InputTwittor input,
           [Service] Kasus2DbContext context,
           [Service] IOptions<KafkaSettings> kafkaSettings)
        {
            var twittor = new Twittor
            {
                UserId = input.UserId,
                Post = input.Post,
                Created = DateTime.Now,
                Updated = DateTime.Now
            };
            var key = "Add-Twit-" + DateTime.Now.ToString();
            var val = JObject.FromObject(twittor).ToString(Formatting.None);
            var result = await KafkaHelper.SendMessage(kafkaSettings.Value, "twittor-add", key, val);
            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            var ret = new TransactionStatus(result, "");
            if (!result)
                ret = new TransactionStatus(result, "Failed to submit data");

            return await Task.FromResult(ret);
        }

        [Authorize(Roles = new[] { "MEMBER" })]
        public async Task<TransactionStatus> AddCommentTwitAsync(
           InputComment input,
           [Service] Kasus2DbContext context,
           [Service] IOptions<KafkaSettings> kafkaSettings)
        {
            var twit = context.Twittors.Where(o => o.Id == input.TwittorId).FirstOrDefault();
            if (twit == null) return await Task.FromResult(new TransactionStatus(false, "Twit not found"));
            var comment = new Comment
            {
                TwittorId = input.TwittorId,
                UserId = input.UserId,
                Comment1 = input.Comment1,
                Created = DateTime.Now,
                Updated = DateTime.Now 
            };

            var key = "Add-Comment-" + DateTime.Now.ToString();
            var val = JObject.FromObject(comment).ToString(Formatting.None);
            var result = await KafkaHelper.SendMessage(kafkaSettings.Value, "comment-add", key, val);
            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            var ret = new TransactionStatus(result, "");
            if (!result)
                ret = new TransactionStatus(result, "Failed to submit data");

            return await Task.FromResult(ret);
        }

        [Authorize(Roles = new[] { "MEMBER" })]
        public async Task<TransactionStatus> DeleteTwitAsync(
            InputTwittorDelete input,
            [Service] Kasus2DbContext context,
            [Service] IOptions<KafkaSettings> kafkaSettings)
        {
            var twittor = context.Twittors.Where(o => o.Id == input.Id).FirstOrDefault();
            if (twittor == null) return await Task.FromResult(new TransactionStatus(false, "Twit not found"));

            var key = "Delete-Twit-" + DateTime.Now.ToString();
            var val = JObject.FromObject(twittor).ToString(Formatting.None);
            var result = await KafkaHelper.SendMessage(kafkaSettings.Value, "twittor-delete", key, val);
            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            var ret = new TransactionStatus(result, "");
            if (!result)
                ret = new TransactionStatus(result, "Failed to submit data");

            return await Task.FromResult(ret);
        }

//tidak perlu
       /* [Authorize(Roles = new[] { "MEMBER", "ADMIN" })]
        public async Task<TransactionStatus> DeleteCommentAsync(
            int commentId,
            InputComment input,
            [Service] Kasus2DbContext context,
            [Service] IOptions<KafkaSettings> kafkaSettings)
        {
            var comment = context.Comments.Where(o => o.Id == commentId).FirstOrDefault();
            if (comment == null) return await Task.FromResult(new TransactionStatus(false, "Comment not found"));

            var key = "Delete-Comment-" + DateTime.Now.ToString();
            var val = JObject.FromObject(comment).ToString(Formatting.None);
            var result = await KafkaHelper.SendMessage(kafkaSettings.Value, "comment-delete", key, val);
            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            var ret = new TransactionStatus(result, "");
            if (!result)
                ret = new TransactionStatus(result, "Failed to submit data");

            return await Task.FromResult(ret);
        }
       */

        /*
        [Authorize(Roles = new[] { "MEMBER", "ADMIN" })]
        public async Task<TransactionStatus> UpdateUserAsync(
            RegisterUser input,
            [Service] Kasus2DbContext context,
            [Service] IOptions<KafkaSettings> kafkaSettings)
        {
            var user = new User();
            if(input.Id == null)
            {

                user = context.Users.Where(user => user.Id == Convert.ToInt32(userId)).SingleOrDefault();
            }

            var user = context.Users.Where(o => o.Id == id).FirstOrDefault();
            if (user == null) return await Task.FromResult(new TransactionStatus(false, "User not found"));
            var newUser = new User ()
            {
                userFind.FullName = input.FullName;
                user.Email = input.Email;
                user.Username = input.Username;
                user.Updated = DateTime.Now;
            }

            var key = "Update-User-Profile-" + DateTime.Now.ToString();
            var val = JObject.FromObject(user).ToString(Formatting.None);
            var result = await KafkaHelper.SendMessage(kafkaSettings.Value, "user-update", key, val);
            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            var ret = new TransactionStatus(result, "");
            if (!result)
                ret = new TransactionStatus(result, "Failed to submit data");

            return await Task.FromResult(ret);
        }
        */

        [Authorize(Roles = new[] { "MEMBER", "ADMIN" })]
        public async Task<TransactionStatus> UpdateUserPasswordAsync(
            int id,
            InputUpdatePasswordUser input,
            [Service] Kasus2DbContext context,
            [Service] IOptions<KafkaSettings> kafkaSettings)
        {
            var user = context.Users.Where(o => o.Id == id).FirstOrDefault();
            if (user == null) return await Task.FromResult(new TransactionStatus(false, "User not found"));

            if (user != null)
            {
                var valid = BCrypt.Net.BCrypt.Verify(input.oldPassword, user.Password);
                if (valid)
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(input.newPassword);
                    user.Updated = DateTime.Now;
                }
                else return new TransactionStatus(false, "Invalid password");
            }

            var key = "Update-User-Password-" + DateTime.Now.ToString();
            var val = JObject.FromObject(user).ToString(Formatting.None);
            var result = await KafkaHelper.SendMessage(kafkaSettings.Value, "user-update", key, val);
            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            var ret = new TransactionStatus(result, "");
            if (!result)
                ret = new TransactionStatus(result, "Failed to submit data");

            return await Task.FromResult(ret);
        }

        [Authorize(Roles = new[] { "ADMIN" })]
        public async Task<TransactionStatus> LockUserAsync(
            int id,
            InputUser input,
            [Service] Kasus2DbContext context,
            [Service] IOptions<KafkaSettings> kafkaSettings)
        {
            var user = context.Users.Where(o => o.Id == id).FirstOrDefault();
            if (user != null)
            {
                user.IsLocked = input.IsLocked;
                user.Updated = DateTime.Now;
            }

            var key = "Lock-Unlock-User-" + DateTime.Now.ToString();
            var val = JObject.FromObject(user).ToString(Formatting.None);
            var result = await KafkaHelper.SendMessage(kafkaSettings.Value, "user-update", key, val);
            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            var ret = new TransactionStatus(result, "");
            if (!result)
                ret = new TransactionStatus(result, "Failed to submit data");

            return await Task.FromResult(ret);
        }

        [Authorize(Roles = new[] { "ADMIN" })]
        public async Task<TransactionStatus> AddUserRoleAsync(
           // int userId,
           InputUserRole input,
           [Service] Kasus2DbContext context,
           [Service] IOptions<KafkaSettings> kafkaSettings)
        {
            var user = context.Users.Where(o => o.Id == input.UserId).FirstOrDefault();
            if (user == null) return await Task.FromResult(new TransactionStatus(false, "User not found"));

            var role = context.Roles.Where(u => u.Id == input.RoleId).FirstOrDefault();
            if (role == null) return await Task.FromResult(new TransactionStatus(false, "Role not found"));

            var userRole = context.UserRoles.Where(userRole => userRole.UserId == input.UserId && userRole.RoleId == input.RoleId).FirstOrDefault();
            if(userRole != null)
            {
                return await Task.FromResult(new TransactionStatus(false, $"UserId {input.UserId} with RoleId {input.RoleId} already exist"));
            }

            var newUserRole = new UserRole
            {
                RoleId = role.Id,
                UserId = user.Id
            };

            var key = "Add-UserRole-" + DateTime.Now.ToString();
            var val = JObject.FromObject(newUserRole).ToString(Formatting.None);
            var result = await KafkaHelper.SendMessage(kafkaSettings.Value, "userRole-add", key, val);
            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            var ret = new TransactionStatus(result, "");
            if (!result)
                ret = new TransactionStatus(result, "Failed to submit data");

            return await Task.FromResult(ret);
        }

        [Authorize(Roles = new[] { "ADMIN" })]
        public async Task<TransactionStatus> UpdateUserRoleAsync(
            //int id,
            InputUserRole input,
            [Service] Kasus2DbContext context,
            [Service] IOptions<KafkaSettings> kafkaSettings)
        {
            var user = context.Users.Where(o => o.Id == input.UserId).FirstOrDefault();
            if (user == null) return await Task.FromResult(new TransactionStatus(false, "User not found"));

            var role = context.Roles.Where(u => u.Id == input.RoleId).FirstOrDefault();
            if (role == null) return await Task.FromResult(new TransactionStatus(false, "Role not found"));

            var userRole = context.UserRoles.Where(userRole => userRole.UserId == input.UserId && userRole.RoleId == input.RoleId).FirstOrDefault();
            if (userRole != null)
            {
                return await Task.FromResult(new TransactionStatus(false, $"UserId {input.UserId} with RoleId {input.RoleId} already exist"));
            }

            var newUserRole = new UserRole
            {
                //Id = userRole.Id,
                UserId = user.Id,
                RoleId = role.Id
            };

            var key = "Update-UserRole-" + DateTime.Now.ToString();
            var val = JObject.FromObject(newUserRole).ToString(Formatting.None);
            var result = await KafkaHelper.SendMessage(kafkaSettings.Value, "userRole-update", key, val);
            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            var ret = new TransactionStatus(result, "");
            if (!result)
                ret = new TransactionStatus(result, "Failed to submit data");

            return await Task.FromResult(ret);
        }

        public async Task<TransactionStatus> RegisterUserAsync(
            RegisterUser input,
            [Service] Kasus2DbContext context,
            [Service] IOptions<KafkaSettings> kafkaSettings)
        {
            var user = context.Users.Where(o => o.Username == input.UserName).FirstOrDefault();
            if (user != null)
            {
                return await Task.FromResult(new TransactionStatus(false, "Username Registered, try with another username"));
            }
            var newUser = new User
            {
                FullName = input.FullName,
                Email = input.Email,
                Username = input.UserName,
                Password = BCrypt.Net.BCrypt.HashPassword(input.Password),
                Created = DateTime.Now,
                Updated = DateTime.Now,
                IsLocked = false
            };

            var key = "New-User-Add-" + DateTime.Now.ToString();
            var val = JObject.FromObject(newUser).ToString(Formatting.None);
            var result = await KafkaHelper.SendMessage(kafkaSettings.Value, "user-add", key, val);
            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            var ret = new TransactionStatus(result, "");
            if (!result)
                ret = new TransactionStatus(result, "Failed to submit data");

            return await Task.FromResult(ret);
        }

       
        

        public async Task<UserToken> LoginAsync(
            LoginUser input,
            [Service] IOptions<TokenSettings> tokenSettings,
            [Service] Kasus2DbContext context)
        {
            var user = context.Users.Where(o => o.Username == input.Username).FirstOrDefault();
            if (user == null)
            {
                return await Task.FromResult(new UserToken(null, null, "Username or password was invalid"));
            }
            bool valid = BCrypt.Net.BCrypt.Verify(input.Password, user.Password);
            if (valid)
            {
                var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.Value.Key));
                var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Name, user.Username));

                var userRoles = context.UserRoles.Where(o => o.UserId == user.Id).ToList();
                foreach (var userRole in userRoles)
                {
                    var role = context.Roles.Where(o => o.Id == userRole.RoleId).FirstOrDefault();
                    if (role != null)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role.Name));
                    }
                }

                var expired = DateTime.Now.AddHours(3);
                var jwtToken = new JwtSecurityToken(
                    issuer: tokenSettings.Value.Issuer,
                    audience: tokenSettings.Value.Audience,
                    expires: expired,
                    claims: claims,
                    signingCredentials: credentials
                );

                return await Task.FromResult(
                    new UserToken(new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    expired.ToString(), null));
                //return new JwtSecurityTokenHandler().WriteToken(jwtToken);
            }

            return await Task.FromResult(new UserToken(null, null, Message: "Username or password was invalid"));
        }



    }
}
