using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CAT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CAT.Controllers
{
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly DBContext _context;
        private readonly AppSettings _appSettings;
        private readonly UserManager<User> _userManager;
        public const double e = 2.71828;

        public UsersController(IOptions<AppSettings> appSettings, UserManager<User> userManager, DBContext context)
        {
            _appSettings = appSettings.Value;
            _userManager = userManager;
            _context = context;
        }

        // GET: api/<controller>
        [HttpPost, Route("[action]")]
        public async Task<ActionResult<AuthenToken>> Login([FromBody]GoogleToken ggToken)
        {
            
            var token = new JwtSecurityToken();
            var result = new AuthenToken();
            using var http = new HttpClient();
            var url = $"https://www.googleapis.com/oauth2/v1/userinfo?alt=json&access_token={ggToken.Token}";
            var stringData = (await http.GetStringAsync(url));
            var data = JsonConvert.DeserializeObject<GoogleUser>(stringData);
            var user = await _userManager.FindByEmailAsync(data.Email);
            if (user == null)
            {
                user = new User
                {
                    UserName = data.Email,
                    Email = data.Email,
                    FullName = data.Name
                };

                var Password = "123abcXyz_";
                var register = await _userManager.CreateAsync(user, Password);
                if (!register.Succeeded)
                {
                    foreach (var item in register.Errors)
                    {
                        ModelState.AddModelError(item.Code, item.Description);
                    }
                    return BadRequest(ModelState);
                }

                token = GenToken(user);
                result = new AuthenToken
                {
                    Id = user.Id,
                    AuthToken = token.RawData,
                    UserName = user.FullName,
                    Continued = false
                };
            }
            else
            {
                var exam = await _context.Exam
                    .Where(x => x.UserID == user.Id && x.EndDate > DateTime.Now && x.Finished == false)
                    .Select(x => x).FirstOrDefaultAsync();

                token = GenToken(user);
                result = new AuthenToken
                {
                    Id = user.Id,
                    AuthToken = token.RawData,
                    UserName = user.FullName,
                    Continued = exam != null
                };
            }
            return Ok(result);
        }

        [HttpGet, Route("[action]")]
        [Authorize]
        public async Task<ActionResult<ContinueExam>> CheckContinueExam()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }
            var exam = await _context.Exam
                    .Where(x => x.UserID == user.Id && x.EndDate > DateTime.Now && x.Finished == false)
                    .Select(x => x).FirstOrDefaultAsync();
            var result = new ContinueExam
            {
                Continued = exam != null
            };
            return Ok(result);
        }
        [HttpGet, Route("[action]")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Result>>> GetResult()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }
            var result = await _context.Exam
                    .Where(x => x.UserID == user.Id)
                    .Select(x => new Result { StartDate = new DateTimeOffset(x.StartDate).ToUnixTimeMilliseconds(), Grade = x.Theta }).ToListAsync();
           
            return Ok(result);
        }

        // GET api/<controller>/5
        [HttpGet, Route("[action]")]
        [Authorize]
        public async Task<ActionResult<QuestionViewModel>> ContinueExam()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var data = await _context.Exam
                .Where(x => x.UserID == user.Id && x.EndDate > DateTime.Now && x.Finished == false)
                .Select(x => x).FirstOrDefaultAsync();

            var listId = data.QuestionID.Split(',').Select(int.Parse);
            var lastQuestionId = listId.Last();

            var ContentQuestion = await _context.Questions
                .Where(x => x.ID == lastQuestionId)
                .Select(x => x.Content).FirstOrDefaultAsync();

            var answers = await _context.Answers
                .Where(x => x.QuestionID == lastQuestionId)
                .Select(x => new AnswerViewModel { IdAnswer = x.ID, ContentAnswer = x.Content }).ToListAsync();

            var result = new QuestionViewModel
            {
                QuestionStt = listId.Count(),
                IdQuestion = lastQuestionId,
                ContentQuestion = ContentQuestion,
                IdExam = data.ID,
                Answers = answers,
                Finished = "cont",
                //EndDate = (long)(data.EndDate - new DateTime(1970, 1, 1)).TotalMilliseconds
                EndDate = new DateTimeOffset(data.EndDate).ToUnixTimeMilliseconds()
        };
            return result;
        }
 
        // POST api/<controller>
        [HttpPost, Route("[action]")]
        [Authorize]
        public async Task<ActionResult<QuestionViewModel>> BeginDoTest([FromBody]LevelModel levelChoose)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var aInterval = new TimeSpan(0, 0, 45, 0);
            var data = new Exam
            {
                UserID = user.Id,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.Add(aInterval),
                Finished = false
            };
            Random rnd = new Random();
            var listQuestion = new List<Questions>();
            var nextQuestion = new Questions();
            var theta = 0.0;
            if (levelChoose.Level == 1)
            {
                theta = 0.5;
                listQuestion = await _context.Questions.Where(x =>  x.b < (-0.5)).ToListAsync();
                var index = rnd.Next(0, listQuestion.Count);
                nextQuestion = listQuestion[index];
            }
                
            else if (levelChoose.Level == 2)
            {
                theta = 1.5;
                listQuestion = await _context.Questions.Where(x => x.b > (-0.4) && x.b < (1.5)).ToListAsync();
                var index = rnd.Next(0, listQuestion.Count);
                nextQuestion = listQuestion[index];
            }
            else
            {
                theta = 2.5;
                listQuestion = await _context.Questions.Where(x => x.b > (1.6)).ToListAsync();
                var index = rnd.Next(0, listQuestion.Count);
                nextQuestion = listQuestion[index];
            }
            

            data.Theta = theta;
            data.SE = 1.0;
            data.SumI = 1.0;
            data.SumS = 0;
            data.QuestionID = nextQuestion.ID.ToString();

            _context.Exam.Add(data);
            await _context.SaveChangesAsync();

            var data1 = await _context.Exam
                .Where(x => x.UserID == user.Id && x.EndDate > DateTime.Now && x.Finished == false)
                .Select(x => x).FirstOrDefaultAsync();

            var contentQuestion = nextQuestion.Content;

            var answers = await _context.Answers
                .Where(x => x.QuestionID == nextQuestion.ID)
                .Select(x => new AnswerViewModel { IdAnswer = x.ID, ContentAnswer = x.Content }).ToListAsync();

            var result = new QuestionViewModel
            {
                QuestionStt = 1,
                IdQuestion = nextQuestion.ID,
                ContentQuestion = contentQuestion,
                IdExam = data1.ID,
                Answers = answers,
                Finished = "cont",
                EndDate = new DateTimeOffset(data.EndDate).ToUnixTimeMilliseconds()
            };
            return result;
            
        }
        [HttpPost, Route("[action]")]
        [Authorize]
        public async Task<ActionResult<QuestionViewModel>> GetNextQuestion([FromBody]AnswerPostModel answerPost)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var rightAnswer = false;
            var exam = await _context.Exam.FindAsync(answerPost.ExamId);

            if(exam.EndDate < DateTime.Now)
            {
                var resultTimeOut = new QuestionViewModel
                {
                    Finished = "timeout"
                };
                return resultTimeOut;
            }

            var answer = await _context.Answers.FindAsync(answerPost.AnswerId);
            if (answer.RightAnswer == true)
                rightAnswer = true;
            var listIdAnsweredQuestion = exam.QuestionID.Split(',').Select(int.Parse);
            var listNewQuestion = await _context.Questions.Where(x => !listIdAnsweredQuestion.Contains(x.ID)).ToListAsync();

            var nextQuestion = new AlgorithmModel();
            if (listNewQuestion == null)
                return NoContent();
            if(listIdAnsweredQuestion.Count() == 1)
            {
                nextQuestion = GetNewQuestion(exam.Theta, exam.SE, exam.SumI, exam.SumS, listNewQuestion, rightAnswer,2);
            }
            else
            {
                if(exam.StatusPreviousAnswer == rightAnswer)
                {
                    
                    if (rightAnswer == true)
                        nextQuestion = GetNewQuestion(exam.Theta, exam.SE, exam.SumI, exam.SumS, listNewQuestion, rightAnswer, 2);
                    else
                        nextQuestion = GetNewQuestion(exam.Theta, exam.SE, exam.SumI, exam.SumS, listNewQuestion, rightAnswer, 3);
                }
                else
                {
                    nextQuestion = GetNewQuestion(exam.Theta, exam.SE, exam.SumI, exam.SumS, listNewQuestion, rightAnswer,1);
                }
            }

            if (nextQuestion.finished)
            {
                exam.Finished = true;
                _context.Entry(exam).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                var resultFinish = new QuestionViewModel
                {
                    Finished = "complete",
                    grade = exam.Theta
                };
                return resultFinish;
            }
            if(listIdAnsweredQuestion.Count() == 30)
            {
                var resultTimeOut = new QuestionViewModel
                {
                    Finished = "notvalue",
                    grade = exam.Theta
                };
                return resultTimeOut;
            }

            exam.Theta = nextQuestion.Theta;
            exam.SE = nextQuestion.SE;
            exam.SumI = nextQuestion.SumI;
            exam.SumS = nextQuestion.SumS;
            exam.QuestionID += "," + nextQuestion.IdQuestion.ToString();
            exam.StatusPreviousAnswer = rightAnswer;
            _context.Entry(exam).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var ContentQuestion = await _context.Questions
              .Where(x => x.ID == nextQuestion.IdQuestion)
              .Select(x => x.Content).FirstOrDefaultAsync();

            var answers = await _context.Answers
                .Where(x => x.QuestionID == nextQuestion.IdQuestion)
                .Select(x => new AnswerViewModel { IdAnswer = x.ID, ContentAnswer = x.Content }).ToListAsync();
            var result = new QuestionViewModel
            {
                QuestionStt = listIdAnsweredQuestion.Count() + 1,
                IdQuestion = nextQuestion.IdQuestion,
                ContentQuestion = ContentQuestion,
                IdExam = exam.ID,
                Answers = answers,
                Finished = "cont",
                EndDate = new DateTimeOffset(exam.EndDate).ToUnixTimeMilliseconds()
            };
            return result;

        }

        private JwtSecurityToken GenToken(User user)
        {
            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                Issuer = "https://localhost:5001",
                Audience = "api",
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return token as JwtSecurityToken;
        }

        private AlgorithmModel GetNewQuestion(double theta, double SE, double SumI, double SumS, List<Questions> listQuestion, bool answerRight,int continued)
        {
            var questions = new Questions();
            bool finished = false;
            
            var Pi = new List<double>();
            var Ii = new List<double>();
            foreach (var question in listQuestion)
            {
                var pi1 = question.c + (1 - question.c) * (Math.Pow(e, (question.a * (theta - question.b))) / (1 + Math.Pow(e, (question.a * (theta - question.b)))));
                Pi.Add(pi1);
                var ii = (question.a * question.a) * (((pi1 - question.c) * (pi1 - question.c)) / ((1 - question.c) * (1 - question.c))) * ((1 - pi1) / pi1);
                Ii.Add(ii);
            }
            var nextQuestion = Ii.Max();
            SumI += nextQuestion;
            var index = Ii.IndexOf(nextQuestion);
            var pNeed = Pi[index];
            questions = listQuestion[index];
            var u = 0.0;
            if (answerRight)
            {
                u = 1.0;
            }
            var Si = (questions.a * (pNeed - questions.c) * (u - pNeed)) / ((1 - questions.c) * pNeed);
            SumS += Si;
            switch (continued)
            {
                case 1:
                    theta += (SumS / SumI);
                    break;
                case 2:
                    theta += 0.25;
                    break;
                case 3:
                    theta -= 0.25;
                    break;
            }
            SE = 1 / Math.Sqrt(SumI);
            if ((SE + theta) < 1.0 || SE < 0.3)
            {
                finished = true;
            }
            
            
            var result = new AlgorithmModel
            {
                Theta = theta,
                SE = SE,
                SumI = SumI,
                SumS = SumS,
                IdQuestion = questions.ID,
                finished = finished
            };
            return result;
        }
    }
}
